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
        readonly ChangeGraphBuilder m_ChangeGraphBuilder;

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
            m_ChangeGraphBuilder = new ChangeGraphBuilder(fileReferenceComparer);
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

            ResetSyncStateIfNecessary(group);

            // get required services
            var syncPointService = group.GetSyncPointService();
            var conflictService = group.GetSyncConflictService();
            var syncActionService = group.GetSyncActionService();            
            
            // for all folders, get the changes since the last sync
            var latestSyncPoint = syncPointService.LatestSyncPoint;
            var diffs = GetDiffs(syncFolders, historyService, latestSyncPoint?.ToSnapshots).ToList();
                
            var newSyncPoint = new MutableSyncPoint()
            {
                Id = GetNextSyncPointId(latestSyncPoint),
                FromSnapshots = latestSyncPoint?.ToSnapshots,
                ToSnapshots = diffs.ToDictionary(d => d.History.Name, d => d.ToSnapshot.Id),
                FilterConfigurations = syncFolders.ToDictionary(f => f.Name, f => f.Filter)
            };                                                         


            var filters = syncFolders.ToDictionary(
                f => f.Name, 
                f => m_FilterFactory.GetFilter(f.Filter),
                StringComparer.InvariantCultureIgnoreCase);
                                                   

            // group changes by files            
            
            var syncStateUpdater = new SyncActionUpdateBuilder();
            

            foreach (var graph in m_ChangeGraphBuilder.GetChangeGraphs(diffs))
            {
                var path = graph.Nodes.First(node => node.Value != null).Value.Path;

                // skip if there is a conflict for the current file
                if (conflictService.ItemExists(path))
                {
                    continue;                    
                }                

                // check if all pending sync actions can be applied to the change graph
                var unapplicaleSyncActions = GetUnapplicableSyncActions(graph, syncActionService[path].Where(IsPendingSyncAction));

                // pending sync actions could not be applied => skip file
                if (unapplicaleSyncActions.Any())
                {
                    // cancel unapplicable actions
                    syncStateUpdater.UpdateSyncActions(unapplicaleSyncActions.Select(a => a.WithState(SyncActionState.Cancelled)));
                   
                    // add a conflict for the file (the snapshot id of the conflict can be determined from the oldest unapplicable sync action)
                    var oldestSyncPointId = unapplicaleSyncActions.Min(a => a.SyncPointId);                    
                    var syncPoint = syncPointService[oldestSyncPointId];                    
                    syncStateUpdater.AddConflict(new ConflictInfo(unapplicaleSyncActions.First().Path, syncPoint.FromSnapshots));
                                        
                    continue;
                }
                
                //in the change graph, detect conflicts
                // if there is only one sink, no conflicts exist
                var sinks = graph.GetSinks().ToArray();
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
                        var currentVersion = diff.ToSnapshot.RootDirectory.GetFileReferenceOrDefault(path);

                        var syncAction = GetSyncAction(targetSyncFolderName, newSyncPoint.Id, currentVersion, sink);
                        if (syncAction != null && filters[targetSyncFolderName].IncludeInResult(syncAction))
                        {
                            syncStateUpdater.AddSyncAction(syncAction);
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
                        syncStateUpdater.UpdateSyncActions(pendingSyncActions.Select(a => a.WithState(SyncActionState.Cancelled)));                        

                        //determine the oldest sync action to determine the snapshot ids for the conflict
                        var syncPointId = pendingSyncActions.Min(x => x.SyncPointId);
                       
                        // generate conflict;
                        syncStateUpdater.AddConflict(new ConflictInfo(path, syncPointService[syncPointId].FromSnapshots));
                    }
                    else
                    {
                        // no pending action => the snapshot ids for the conflict are the start snapshots of the current sync

                        // generate conflict
                        var fromSnapshots = diffs.ToDictionary(d => d.History.Name, d => d.FromSnapshot?.Id);
                        var conflictInfo = new ConflictInfo(path, fromSnapshots);
                        syncStateUpdater.AddConflict(conflictInfo);
                    }
                }
            }

            // save actions, conflicts and sync point          
            syncPointService.AddItem(newSyncPoint);            
            syncStateUpdater.Apply(syncActionService, conflictService);
                        
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


        void ResetSyncStateIfNecessary(IGroup group)
        {
            var syncPointService = group.GetSyncPointService();
            var syncActionService = group.GetSyncActionService();
            var conflictService = group.GetSyncConflictService();

            var syncFolders = group.GetConfigurationService().Items.ToArray();
                
            var latestSyncPoint = syncPointService.LatestSyncPoint;

            if (ContainsNewFolders(syncFolders, latestSyncPoint) || WasFilterModified(syncFolders, latestSyncPoint))
            {
                // insert "Reset" sync pint
                var resetSyncPoint = new MutableSyncPoint()
                {
                    Id = GetNextSyncPointId(latestSyncPoint),
                    FromSnapshots = latestSyncPoint?.ToSnapshots,
                    ToSnapshots = null,
                    FilterConfigurations = syncFolders.ToDictionary(f => f.Name, f => f.Filter)
                };

                syncPointService.AddItem(resetSyncPoint);

                // cancel all pending sync actions
                var cancelledSyncActions = syncActionService.PendingItems
                    .Select(a => a.WithState(SyncActionState.Cancelled));

                syncActionService.UpdateItems(cancelledSyncActions);

                // remove all conflicts                
                conflictService.RemoveItems(conflictService.Items.ToArray());                
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
            if (changeGraph.Contains(action.FromVersion) && changeGraph.Contains(action.ToVersion))
            {
                changeGraph.AddEdge(action.FromVersion, action.ToVersion);
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
                    return SyncAction.CreateRemoveFileSyncAction(targetName, SyncActionState.Queued, syncPointId, currentFileVersion);
                }
                else
                {
                    return SyncAction.CreateReplaceFileSyncAction(targetName, SyncActionState.Queued, syncPointId, currentFileVersion, newFileVersion);
                }
            }
            else
            {
                if (newFileVersion != null)
                {
                    return SyncAction.CreateAddFileSyncAction(targetName, SyncActionState.Queued, syncPointId, newFileVersion);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
        }

    }
}