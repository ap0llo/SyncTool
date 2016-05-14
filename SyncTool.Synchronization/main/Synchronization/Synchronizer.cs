// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;
using SyncTool.Configuration;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.ChangeGraph;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;


namespace SyncTool.Synchronization
{
    public class Synchronizer : ISynchronizer
    {        
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly IChangeFilterFactory m_FilterFactory;


        public Synchronizer(IEqualityComparer<IFileReference> fileReferenceComparer, IChangeFilterFactory filterFactory)
        {
            if (fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }
            if (filterFactory == null)
            {
                throw new ArgumentNullException(nameof(filterFactory));
            }
            m_FileReferenceComparer = fileReferenceComparer;
            m_FilterFactory = filterFactory;
        }
        

        public void Synchronize(IGroup group)
        {
            var syncFolders = group.GetConfigurationService().Items.ToArray();

            // there need to be at least 2 sync folders, otherwise, syncing makes no sense
            if (syncFolders.Length < 2)
            {
                return;
            }

            // for every folder, there needs to be at least one snapshot
            var historyService = group.GetService<IHistoryService>();
            if(!syncFolders.All(f => historyService.ItemExists(f.Name) && historyService[f.Name].Snapshots.Any()))
            {
                return;                
            }

            // get required services
            var syncPointService = group.GetSyncPointService();
            var conflictService = group.GetSyncConflictService();
            var syncActionService = group.GetSyncActionService();
            
            // for all histories, get the changes since the last sync
            var latestSyncPoint = syncPointService.LatestSyncPoint;

            List<IFileSystemDiff> diffs;
            MutableSyncPoint newSyncPoint;

            if (ContainsNewFolders(syncFolders, latestSyncPoint) || WasFilterModified(syncFolders, latestSyncPoint))
            {                
                diffs = GetDiffs(syncFolders, historyService, null).ToList();

                newSyncPoint = new MutableSyncPoint()
                {
                    Id = GetNextSyncPointId(latestSyncPoint),
                    FromSnapshots = null,
                    ToSnapshots = diffs.ToDictionary(d => d.History.Name, d => d.ToSnapshot.Id),
                    FilterConfigurations = syncFolders.ToDictionary(f => f.Name, f => f.Filter)
                };

                // cancel all pending sync actions
                var cancelledSyncActions = syncActionService.PendingItems                    
                    .Select(a => a.WithState(SyncActionState.Cancelled));

                syncActionService.UpdateItems(cancelledSyncActions);

                // remove all conflicts                
                conflictService.RemoveItems(conflictService.Items.ToArray());
            }
            else
            {
                diffs = GetDiffs(syncFolders, historyService, latestSyncPoint?.ToSnapshots).ToList();
                
                newSyncPoint = new MutableSyncPoint()
                {
                    Id = GetNextSyncPointId(latestSyncPoint),
                    FromSnapshots = latestSyncPoint?.ToSnapshots,
                    ToSnapshots = diffs.ToDictionary(d => d.History.Name, d => d.ToSnapshot.Id),
                    FilterConfigurations = syncFolders.ToDictionary(f => f.Name, f => f.Filter)
                };
            }                                              


            var filters = syncFolders.ToDictionary(
                f => f.Name, 
                f => m_FilterFactory.GetFilter(f.Filter),
                StringComparer.InvariantCultureIgnoreCase);
                                                   

            // group changes by files
            var changeListsByFile = diffs
                .SelectMany(diff => diff.ChangeLists.Select(cl => new ChangeListWithHistoryName(diff.History.Name, cl)))
                .GroupBy(x => x.Path);
            
            
            var newConflicts = new Dictionary<string, ConflictInfo>();
            var newSyncActions = new List<SyncAction>();
            var updatedSyncActions = new List<SyncAction>();

            foreach (var changeLists in changeListsByFile)
            {
                var path = changeLists.Key;

                // skip if there is a conflict for the current file
                if (conflictService.ItemExists(path))
                {
                    continue;                    
                }

                // build change graph from lists
                var changeGraph = GetChangeGraph(diffs, changeLists);

                // check if all pending sync actions can be applied to the change grpah
                var unapplicaleSyncActions = GetUnapplicableSyncActions(changeGraph, syncActionService[path].Where(IsPendingSyncAction));

                // pending sync actions could not be applied => skip file
                if (unapplicaleSyncActions.Any())
                {
                    // cancel unapplicable actions
                    updatedSyncActions.AddRange(unapplicaleSyncActions.Select(a => a.WithState(SyncActionState.Cancelled)));
                   
                    // add a conflict for the file (the snapshot id of the conflict can be determined from the oldest unapplicable sync action)
                    var oldestSyncPointId = unapplicaleSyncActions.Min(a => a.SyncPointId);                    
                    var syncPoint = syncPointService[oldestSyncPointId];                    
                    newConflicts.Add(path, new ConflictInfo(unapplicaleSyncActions.First().Path, syncPoint.FromSnapshots));
                                        
                    continue;
                }

                
                //in the change graph, detect conflicts
                // if there is only one sink, no conflicts exist
                var sinks = changeGraph.GetSinks().ToArray();
                if (!sinks.Any())
                {
                    // not possible (in this case the graph would be empty, which cannot happen)
                    throw new InvalidOperationException();
                }

                if (sinks.Length == 1)
                {                 
                    // no conflict => generate sync actions, to replace the outdated file versions or add the file to a target

                    var sink = sinks.Single();
                    
                    foreach (var diff in diffs)
                    {
                        var targetSyncFolderName = diff.History.Name;
                        var currentRoot = diff.ToSnapshot.RootDirectory;
                        var currentVersion = currentRoot.FileExists(path) 
                            ? currentRoot.GetFile(path).ToReference() 
                            : null;

                        var syncAction = GetSyncAction(targetSyncFolderName, newSyncPoint.Id, currentVersion, sink);
                        if (syncAction != null && filters[targetSyncFolderName].IncludeInResult(syncAction))
                        {
                            newSyncActions.Add(syncAction);
                        }
                    } 
                }
                else
                {
                    // multiple sinks in the change graph => conflict

                    // if there are pending actions for this file, they need to be cancelled
                    var pendingSyncActions = syncActionService[path]
                        .Where(IsPendingSyncAction)
                        .ToArray();

                    if (pendingSyncActions.Any())
                    {
                        //cancel actions          
                        updatedSyncActions.AddRange(pendingSyncActions.Select(a => a.WithState(SyncActionState.Cancelled)));                        

                        //determine the oldest sync action to determine the snapshot ids for the conflict
                        var syncPointId = pendingSyncActions.Min(x => x.SyncPointId);
                       
                        // generate conflict
                        var conflictInfo = new ConflictInfo(path, syncPointService[syncPointId].FromSnapshots);
                        newConflicts.Add(path, conflictInfo);
                    }
                    else
                    {
                        // no pending action => the snapshot ids for the conflict are the start snapshots of the current sync

                        // generate conflict
                        var conflictInfo = new ConflictInfo(path, diffs.Where(d => d.FromSnapshot != null).ToDictionary(d => d.History.Name, d => d.FromSnapshot.Id));
                        newConflicts.Add(path, conflictInfo);
                    }
                }
            }

            // save actions, conflicts and sync point
          
            syncPointService.AddItem(newSyncPoint);

            conflictService.AddItems(newConflicts.Values);

            syncActionService.AddItems(newSyncActions);
            syncActionService.UpdateItems(updatedSyncActions);            
        }
        

        /// <summary>
        /// Gets the diffs relevant for the next synchronization (all changes since the last sync point)
        /// </summary>
        IEnumerable<IFileSystemDiff> GetDiffs(IEnumerable<SyncFolder> syncFolders, IHistoryService historyService, IReadOnlyDictionary<string, string> toSnapshots )
        {
            foreach (var syncFolder in syncFolders)
            {
                var history = historyService[syncFolder.Name];

                var diff = toSnapshots == null
                    // no saved snapshot ids from last sync => get all the changes from the initial commit
                    ? history.GetChanges(history.LatestFileSystemSnapshot.Id)
                    // toSnapshots != null => get all the changes since the last sync
                    : history.GetChanges(toSnapshots[syncFolder.Name], history.LatestFileSystemSnapshot.Id);

                var filter = m_FilterFactory.GetFilter(syncFolder.Filter);

                yield return new FilteredFileSystemDiff(diff, filter);
            }                
            
        }


        bool ContainsNewFolders(IEnumerable<SyncFolder> syncFolders , ISyncPoint syncPoint)
        {
            if (syncPoint == null)
            {
                return false;
            }
            else
            {
                var syncFolderNames = syncFolders.Select(f => f.Name).ToArray();
                return syncFolderNames.Any(name => !syncPoint.ToSnapshots.ContainsKey(name));
            }
        }

        bool WasFilterModified(IEnumerable<SyncFolder> syncFolders, ISyncPoint lastSyncPoint)
        {
            if (lastSyncPoint == null)
            {
                return false;
            }
            else
            {
                var filters = lastSyncPoint.FilterConfigurations;
                return syncFolders.Any(folder =>                
                    !filters.ContainsKey(folder.Name) || !filters[folder.Name].Equals(folder.Filter)
                );
            }
        }

        /// <summary>
        /// Determine the id of the next sync point to be stored
        /// </summary>
        int GetNextSyncPointId(ISyncPoint currentSyncPoint)
        {
            // currentSyncPoint may be null
            return currentSyncPoint?.Id + 1 ?? 1;
        }

        /// <summary>
        /// Builds a graph of all file versions based on the specified changes
        /// </summary>
        /// <param name="diffs">The diffs the changes were taken from</param>
        /// <param name="changeLists">The changes to be included in the graph</param>
        Graph<IFileReference> GetChangeGraph(IEnumerable<IFileSystemDiff> diffs, IEnumerable<ChangeListWithHistoryName> changeLists)
        {
            changeLists = changeLists.ToList();
            diffs = diffs.ToList();

            var path = changeLists.First().Path;

            var graph = new Graph<IFileReference>(m_FileReferenceComparer);
            
            // add ToVersion and FromVersion for every change to the graph
            var changes = changeLists.SelectMany(cl => cl.Changes).ToArray();            
            graph.AddNodes(changes.Select(c => c.FromVersion));
            graph.AddNodes(changes.Select(c => c.ToVersion));

            // for each diff which has no changes, add the current file as node
            var historiesWithoutChanges = diffs
                .Select(d => d.History.Name)
                .Except(changeLists.Select(cl => cl.HistoryName), StringComparer.InvariantCultureIgnoreCase);
        
            foreach (var historyName in historiesWithoutChanges)
            {
                var rootDirectory = diffs.Single(d => d.History.Name.Equals(historyName)).ToSnapshot.RootDirectory;
                if (rootDirectory.FileExists(path))
                {
                    graph.AddNodes(rootDirectory.GetFile(path).ToReference());
                }
                else
                {
                    graph.AddNodes((IFileReference)null);
                }
            }

            // add all edges to the graph
            foreach (var change in changes)
            {
                graph.AddEdge(change.FromVersion, change.ToVersion);
            }

            return graph;
        }

        IList<SyncAction> GetUnapplicableSyncActions(Graph<IFileReference> changeGraph, IEnumerable<SyncAction> syncActions)
        {
            // try to apply pending actions to the graph
            var unapplicableSyncActions = new List<SyncAction>();
            foreach (var syncAction in syncActions)
            {
                if (!TryApplySyncAction(changeGraph, syncAction))
                {
                    // a pending action cannot be applied to the graph => a conflict exists
                    unapplicableSyncActions.Add(syncAction);
                }
            }

            return unapplicableSyncActions;
        }

        /// <summary>
        /// Tries to apply the specified sync action to the change grpah
        /// </summary>
        /// <returns>Retruns true if the action could be added to the graph, otherwise return false</returns>
        bool TryApplySyncAction(Graph<IFileReference> changeGraph, SyncAction action)
        {
            dynamic dynamicAction = action;
            return TryApplySyncAction(changeGraph, dynamicAction);
        }

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, AddFileSyncAction action)
        {
            if (changeGraph.Contains(null) && changeGraph.Contains(action.ToVersion))
            {
                changeGraph.AddEdge(null, action.ToVersion);
                return true;
            }
            return false;
        }

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, ReplaceFileSyncAction action)
        {
            if (changeGraph.Contains(action.FromVersion) && changeGraph.Contains(action.ToVersion))
            {
                changeGraph.AddEdge(action.FromVersion, action.ToVersion);
                return true;
            }

            return false;
        }

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, RemoveFileSyncAction action)
        {
            if (changeGraph.Contains(action.FromVersion) && changeGraph.Contains(null))
            {
                changeGraph.AddEdge(action.FromVersion, null);
                return true;
            }
            return false;
        }

        bool IsPendingSyncAction(SyncAction action) => action.State.IsPendingState();

        SyncAction GetSyncAction(string targetName, int syncPointId, IFileReference currentFileVersion, IFileReference newFileVersion)
        {
            if (m_FileReferenceComparer.Equals(currentFileVersion, newFileVersion))
            {
                return null;
            }

            if (currentFileVersion != null)
            {
                if (newFileVersion == null)
                {
                    return new RemoveFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, syncPointId, currentFileVersion);
                }
                else
                {
                    return new ReplaceFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, syncPointId, currentFileVersion, newFileVersion);
                }
            }
            else
            {
                if (newFileVersion != null)
                {
                    return new AddFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, syncPointId, newFileVersion);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

    }
}