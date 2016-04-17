// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;
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

        public Synchronizer(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if (fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }
            m_FileReferenceComparer = fileReferenceComparer;
        }



        public void Synchronize(IGroup group)
        {
            var historyService = group.GetService<IHistoryService>();

            // there need to be at least 2 histories, otherwise, syncing makes no sense
            if (historyService.Items.Count() < 2)
            {
                return;
            }

            // for every history, there needs to be at least one snapshot
            if(!historyService.Items.All(h => h.Snapshots.Any()))
            {
                return;                
            }


            var syncPointService = group.GetService<ISyncPointService>();
            var conflictService = group.GetService<IConflictService>();
            var syncActionService = group.GetService<ISyncActionService>();
            
            // for all histories, get the changes since the last sync
            var latestSyncPoint = syncPointService.LatestSyncPoint;
            var diffs = GetDiffs(historyService, latestSyncPoint).ToList();            
            var newSyncPoint = new MutableSyncPoint()
            {
                Id = GetNextSyncPointId(latestSyncPoint),
                FromSnapshots = latestSyncPoint?.ToSnapshots,
                ToSnapshots = diffs.ToDictionary(d => d.History.Name, d => d.ToSnapshot.Id)
            };

            // group changes by files
            var changeListsByFile = diffs
                .SelectMany(diff => diff.ChangeLists.Select(cl => new ChangeListWithHistoryName(diff.History.Name, cl)))
                .GroupBy(x => x.Path);
            
            //TODO: Auto-resolve conflicts when possible

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

                // try to apply pending actions to the graph
                var nonApplicaleSyncActions = new List<SyncAction>();
                foreach (var syncAction in syncActionService[path])
                {
                    if (!TryApplySyncAction(changeGraph, syncAction))
                    {
                        // a pending action cannot be applied to the graph => a conflict exists
                        nonApplicaleSyncActions.Add(syncAction);     
                        
                        // this sync action is no longer applicable and will be canceled
                        updatedSyncActions.Add(syncAction.WithState(SyncActionState.Cancelled));
                    }
                }

                // pending sync actions could not be applied => skip file
                if (nonApplicaleSyncActions.Any())
                {
                    var oldestSyncPointId = nonApplicaleSyncActions.Min(a => a.SyncPointId);
                    var oldestSyncAction = nonApplicaleSyncActions.Single(a => a.SyncPointId == oldestSyncPointId);
                    newConflicts.Add(path, CreateConflictInfo(syncPointService, oldestSyncAction));
                                        
                    continue;
                }

                
                //in the change graph, detect conflicts
                // if there is only one sink, no conflicts exist
                var sinks = changeGraph.GetSinks().ToArray();
                if (!sinks.Any())
                {
                    // not possible
                    throw new InvalidOperationException();
                }

                if (sinks.Length == 1)
                {
                    //TODO: do we need to check if the sink is reachable from all current node????

                    // no conflict => generate sync actions

                    var sink = sinks.Single();

                    foreach (var diff in diffs)
                    {
                        var targetName = diff.History.Name;
                        var currentRoot = diff.ToSnapshot.RootDirectory;
                        var currentVersion = currentRoot.FileExists(path) ? currentRoot.GetFile(path).ToReference() : null;

                        if (!m_FileReferenceComparer.Equals(currentVersion, sink))
                        {
                            if (currentVersion != null)
                            {
                                if (sink == null)
                                {
                                    newSyncActions.Add(new RemoveFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, newSyncPoint.Id, currentVersion));
                                }
                                else
                                {
                                    newSyncActions.Add(new ReplaceFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, newSyncPoint.Id, currentVersion, sink));
                                }
                            }
                            else
                            {
                                if (sink != null)
                                {
                                    newSyncActions.Add(new AddFileSyncAction(Guid.NewGuid(), targetName, SyncActionState.Queued, newSyncPoint.Id, sink));
                                }
                                else
                                {
                                    throw new InvalidOperationException();
                                }
                            }                         
                        }                        
                    } 
                }
                else
                {
                    // multiple sinks in the change graph => conflict


                    // if there are pending actions for this file, they need to be cancelled
                    var pendingSyncActions = syncActionService[path]
                        .Where(a => a.State == SyncActionState.Queued || a.State == SyncActionState.Active)
                        .ToArray();

                    if (pendingSyncActions.Any())
                    {
                        //cancel actions          
                        foreach (var syncAction in pendingSyncActions)
                        {
                            updatedSyncActions.Add(syncAction.WithState(SyncActionState.Cancelled));
                        }

                        //determine the oldest sync action to determine the snapshot ids for the conflict
                        var syncPointId = pendingSyncActions.Min(x => x.SyncPointId);
                        var ids = syncPointService[syncPointId].FromSnapshots;

                        // generate conflict
                        var conflictInfo = new ConflictInfo(path, ids);
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





        IEnumerable<IFileSystemDiff> GetDiffs(IHistoryService historyService, ISyncPoint syncPoint)
        {
            //TODO: Handle histories added since the last sync

            if (syncPoint == null)
            {
                // no sync point found => sync was never executed before
                return historyService.Items
                    .Select(h => h.GetChanges(h.LatestFileSystemSnapshot.Id));
            }
            else
            {
                return historyService.Items
                    .Select(h => h.GetChanges(syncPoint.ToSnapshots[h.Name], h.LatestFileSystemSnapshot.Id));
            }            
        }


        int GetNextSyncPointId(ISyncPoint currentSyncPoint)
        {
            // currentSyncPoint may be null
            return currentSyncPoint?.Id + 1 ?? 1;
        }



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

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, SyncAction action)
        {
            dynamic dynamicAction = action;
            return TryApplySyncAction(changeGraph, dynamicAction);
        }

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, AddFileSyncAction action)
        {
            if (changeGraph.Contains(null) && changeGraph.Contains(action.NewFile))
            {
                changeGraph.AddEdge(null, action.NewFile);
                return true;
            }
            return false;
        }

        bool TryApplySyncAction(Graph<IFileReference> changeGraph, ReplaceFileSyncAction action)
        {
            if (changeGraph.Contains(action.OldVersion) && changeGraph.Contains(action.NewVersion))
            {
                changeGraph.AddEdge(action.OldVersion, action.NewVersion);
                return true;
            }
            return false;

        }
        bool TryApplySyncAction(Graph<IFileReference> changeGraph, RemoveFileSyncAction action)
        {
            if (changeGraph.Contains(action.RemovedFile) && changeGraph.Contains(null))
            {
                changeGraph.AddEdge(action.RemovedFile, null);
                return true;
            }
            return false;
        }

        ConflictInfo CreateConflictInfo(ISyncPointService syncPointService, SyncAction action)
        {
            var syncPoint = syncPointService[action.SyncPointId];
            return new ConflictInfo(action.FilePath, syncPoint.FromSnapshots);
        }

        
    }
}