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
            var nextSyncPointId = GetNextSyncPointId(latestSyncPoint);

            var diffs = GetDiffs(historyService, latestSyncPoint);

            // group changes by files
            var changeListsByFile = diffs.Values
                .SelectMany(diff => diff.ChangeLists.Select(cl => new { HistoryName = diff.History.Name, ChangeList = cl}))
                .GroupBy(x => x.ChangeList.Path);
            
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
                var changeGraph = GetChangeGraph(changeLists.Select(x => x.ChangeList));

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
                    newConflicts.Add(path, CreateConflictInfo(oldestSyncAction));
                                        
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

                    foreach (var target in diffs.Keys)
                    {
                        var currentRoot = diffs[target].ToSnapshot.RootDirectory;
                        var currentVersion = currentRoot.FileExists(path) ? currentRoot.GetFile(path).ToReference() : null;

                        if (!m_FileReferenceComparer.Equals(currentVersion, sink))
                        {
                            if (currentVersion != null)
                            {
                                if (sink == null)
                                {
                                    newSyncActions.Add(new RemoveFileSyncAction(Guid.NewGuid(), target, SyncActionState.Queued, nextSyncPointId, currentVersion));
                                }
                                else
                                {
                                    newSyncActions.Add(new ReplaceFileSyncAction(Guid.NewGuid(), target, SyncActionState.Queued, nextSyncPointId, currentVersion, sink));
                                }
                            }
                            else
                            {
                                if (sink != null)
                                {
                                    newSyncActions.Add(new AddFileSyncAction(Guid.NewGuid(), target, SyncActionState.Queued, nextSyncPointId, sink));
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
                    //TODO: Undo Application of pending sync actions (and update the conflict's ids accordingly)
                    //TODO: cancel actions          
                    
                    // generate conflict
                    var conflictInfo = new ConflictInfo(path, diffs.ToDictionary(d => d.Key, d => d.Value.FromSnapshot.Id));
                    newConflicts.Add(path, conflictInfo);
                }

            }

            // save actions, conflicts and sync point

            var newSyncPoint = new MutableSyncPoint()
            {
                Id = nextSyncPointId,
                FromSnapshots = latestSyncPoint?.ToSnapshots,
                ToSnapshots = diffs.ToDictionary(d => d.Key, d => d.Value.ToSnapshot.Id)
            };
            syncPointService.AddItem(newSyncPoint);

            conflictService.AddItems(newConflicts.Values);

            syncActionService.AddItems(newSyncActions);
            syncActionService.UpdateItems(updatedSyncActions);
            
        }





        IDictionary<string, IFileSystemDiff> GetDiffs(IHistoryService historyService, ISyncPoint syncPoint)
        {
            //TODO: Handle histories added since the last sync

            if (syncPoint == null)
            {
                // no sync point found => sync was never executed before
                return historyService.Items
                    .Select(h => h.GetChanges(h.LatestFileSystemSnapshot.Id))
                    .ToDictionary(diff => diff.History.Name);
            }
            else
            {
                return historyService.Items
                    .Select(h => h.GetChanges(syncPoint.ToSnapshots[h.Name], h.LatestFileSystemSnapshot.Id))
                    .ToDictionary(diff => diff.History.Name);
            }            
        }


        int GetNextSyncPointId(ISyncPoint currentSyncPoint)
        {
            // currentSyncPoint may be null
            return currentSyncPoint?.Id + 1 ?? 1;
        }



        Graph<IFileReference> GetChangeGraph(IEnumerable<IChangeList> changeLists)
        {
            var graph = new Graph<IFileReference>(m_FileReferenceComparer);

            foreach (var change in changeLists.SelectMany(cl => cl.Changes))
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

        ConflictInfo CreateConflictInfo(SyncAction action)
        {
            throw new NotImplementedException();
        }

        
    }
}