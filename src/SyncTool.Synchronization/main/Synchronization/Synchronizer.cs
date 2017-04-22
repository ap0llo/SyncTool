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
               
        readonly SyncActionFactory m_SyncActionFactory;

        public Synchronizer(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if (fileReferenceComparer == null)
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            
            m_FileReferenceComparer = fileReferenceComparer;            
            m_SyncActionFactory = new SyncActionFactory(fileReferenceComparer);
        }
        

        public void Synchronize(IGroup group)
        {
            var syncFolders = group.GetConfigurationService().Items.ToArray();

            // there need to be at least 2 sync folders, otherwise, syncing makes no sense
            if (syncFolders.Length < 2)
            {
                return;
            }

            var historyService = group.GetService<IMultiFileSystemHistoryService>();
            historyService.CreateSnapshot();

            // we cannot sync if there isn't at least one snapshot
            if(!historyService.Snapshots.Any())
            {
                return;                
            }
            
            // we cannot sync if there is not at least one snapshot per history
            if (historyService.LatestSnapshot.HistoryNames.Any(name => historyService.LatestSnapshot.GetSnapshot(name) == null))
            {
                return;
            }

            // get required services
            var syncPointService = group.GetSyncPointService();
            var conflictService = group.GetSyncConflictService();
            var syncActionService = group.GetSyncActionService();

            
            var latestSyncPoint = syncPointService.LatestSyncPoint;
            var diff = GetDiff(historyService, latestSyncPoint);

            var wasReset = ResetSyncStateIfNecessary(group, diff);
            if (wasReset)
            {
                diff = GetDiff(historyService, latestSyncPoint);
                latestSyncPoint = syncPointService.LatestSyncPoint;
            }
            
            // for all folders, get the changes since the last sync
                
            var newSyncPoint = new MutableSyncPoint()
            {
                Id = GetNextSyncPointId(syncPointService.LatestSyncPoint),                                
                MultiFileSystemSnapshotId = diff.ToSnapshot.Id
            };

            
                                                   
            
            var syncStateUpdater = new SyncActionUpdateBuilder();
            var changeGraphBuilder = new ChangeGraphBuilder(m_FileReferenceComparer);

            foreach (var graph in changeGraphBuilder.GetChangeGraphs(diff))
            {
                var path = graph.ValueNodes.First(node => node.Value != null).Value.Path;

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
                    var snapshotId = oldestSyncPointId > 1 
                        ? syncPointService[oldestSyncPointId - 1].MultiFileSystemSnapshotId
                        : null;                    
                    syncStateUpdater.AddConflict(new ConflictInfo(unapplicaleSyncActions.First().Path, snapshotId));
                                        
                    continue;
                }
                
                //in the change graph, detect conflicts
                // if there is only one sink, no conflicts exist
                var acylicGraph = graph.ToAcyclicGraph();
                var sinks = acylicGraph.GetSinks().ToArray();
                if (!sinks.Any())
                {
                    // not possible (in this case the graph would be empty, which cannot happen)
                    throw new InvalidOperationException();
                }

                if (sinks.Length == 1)
                {                 
                    // no conflict => generate sync actions, to replace the outdated file versions or add the file to a target

                    var sink = sinks.Single();

                    //TODO: Use C# 7 tuples after it was released
                    IEnumerable<Tuple<string, IFile>> currentFiles = diff.ToSnapshot.GetFiles(path);                
                    foreach (var historyFileTuple in currentFiles)
                    {
                        var targetSyncFolderName = historyFileTuple.Item1;
                        var currentVersion = historyFileTuple.Item2;

                        var syncAction = m_SyncActionFactory.GetSyncAction(targetSyncFolderName, newSyncPoint.Id, currentVersion?.ToReference(), sink);
                        if (syncAction != null)
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
                        var snapshotId = syncPointId > 1
                            ? syncPointService[syncPointId - 1].MultiFileSystemSnapshotId
                            : null;

                        // generate conflict;
                        syncStateUpdater.AddConflict(new ConflictInfo(path, snapshotId));
                    }
                    else
                    {
                        // no pending action => the snapshot ids for the conflict are the start snapshots of the current sync

                        // generate conflict                        
                        var conflictInfo = new ConflictInfo(path, diff.FromSnapshot?.Id);
                        syncStateUpdater.AddConflict(conflictInfo);
                    }
                }
            }

            // save actions, conflicts and sync point          
            syncPointService.AddItem(newSyncPoint);            
            syncStateUpdater.Apply(syncActionService, conflictService);
                        
        }
        

        
        IMultiFileSystemDiff GetDiff(IMultiFileSystemHistoryService historyService, ISyncPoint syncPoint)
        {
            var fromSnapshotId = syncPoint?.MultiFileSystemSnapshotId;
            var toSnapshotId = historyService.LatestSnapshot.Id;

            var diff = fromSnapshotId == null 
                ? historyService.GetChanges(toSnapshotId) 
                : historyService.GetChanges(fromSnapshotId, toSnapshotId);

            return diff;
        }

        bool ResetSyncStateIfNecessary(IGroup group, IMultiFileSystemDiff diff)
        {
            var syncPointService = group.GetSyncPointService();
            var syncActionService = group.GetSyncActionService();
            var conflictService = group.GetSyncConflictService();

            var syncFolders = group.GetConfigurationService().Items.ToArray();
                
            var latestSyncPoint = syncPointService.LatestSyncPoint;

            if (ContainsNewFolders(diff) || WasFilterModified(syncFolders, latestSyncPoint))
            {
                // insert "Reset" sync point
                var resetSyncPoint = new MutableSyncPoint()
                {
                    Id = GetNextSyncPointId(latestSyncPoint),                    
                    MultiFileSystemSnapshotId = null
                };

                syncPointService.AddItem(resetSyncPoint);

                // cancel all pending sync actions
                var cancelledSyncActions = syncActionService.PendingItems
                    .Select(a => a.WithState(SyncActionState.Cancelled));

                syncActionService.UpdateItems(cancelledSyncActions);

                // remove all conflicts                
                conflictService.RemoveItems(conflictService.Items.ToArray());


                return true;
            }

            return false;

        }

        bool ContainsNewFolders(IMultiFileSystemDiff diff)
        {            
            return diff.FromSnapshot != null && diff.HistoryChanges.Any(c => c.Type == ChangeType.Added);
        }

        bool WasFilterModified(IEnumerable<SyncFolder> syncFolders, ISyncPoint lastSyncPoint)
        {
            return false;
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
            //TODO: ensure that all sources of actions still exist

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


    }
}