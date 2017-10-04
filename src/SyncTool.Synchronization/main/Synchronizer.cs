using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using JetBrains.Annotations;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;
using SyncTool.Utilities;

namespace SyncTool.Synchronization
{
    public class Synchronizer : ISynchronizer
    {
        readonly ILogger<Synchronizer> m_Logger;
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        readonly ISyncStateService m_SyncStateService;
        readonly IMultiFileSystemHistoryService m_MultiFileSystemHistoryService;

        public Synchronizer(
            [NotNull] ILogger<Synchronizer> logger,
            [NotNull] IEqualityComparer<IFileReference> fileReferenceComparer,
            [NotNull] ISyncStateService syncStateService,
            [NotNull] IMultiFileSystemHistoryService multiFileSystemHistoryService)
        {
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_FileReferenceComparer = fileReferenceComparer ?? throw new ArgumentNullException(nameof(fileReferenceComparer));
            m_SyncStateService = syncStateService ?? throw new ArgumentNullException(nameof(syncStateService));
            m_MultiFileSystemHistoryService = multiFileSystemHistoryService ?? throw new ArgumentNullException(nameof(multiFileSystemHistoryService));
        }


        public void Run()
        {
            using (m_Logger.BeginScope("Synchronize"))
            using (var updater = m_SyncStateService.BeginUpdate())
            {
                m_Logger.LogInformation("Running sychronization");

                // get changes since last sync
                var diff = GetDiff(updater);

                // removing histories is currently not supported
                if (diff.HistoryChanges.Any(c => c.Type == ChangeType.Deleted))
                {
                    throw new NotImplementedException();
                }

                // no changed files since last sync => sync finished
                if (!diff.FileChanges.Any())
                {
                    m_Logger.LogInformation("No changes since last sync. Sync finished");
                    return;
                }

                // run synchronization for all changed files
                foreach (var changeList in diff.FileChanges)
                {
                    Synchronize(diff, changeList, updater);
                }

                // apply update
                var success = updater.TryApply();

                // log result
                if (success)
                    m_Logger.LogInformation($"Synchronization complete, " +
                                            $"{updater.AddedSyncActions} actions added," +
                                            $"{updater.RemovedSyncActions} actions removed, " +
                                            $"{updater.AddedConflicts} conflicts added, " +
                                            $"{updater.RemovedConflicts} conflicts removed");
                else
                    m_Logger.LogInformation("Could not apply update to sync state, synchronization was rolled back.");
            }
        }


        IMultiFileSystemDiff GetDiff(ISyncStateUpdater updater)
        {
            // load the id of the snapshot used the for the last sync
            var lastSnapshotId = updater.LastSyncSnapshotId;
            m_Logger.LogInformation($"Last sync at multi-filesystem snapshot {lastSnapshotId}");

            // create new snapshot 
            var newSnapshot = m_MultiFileSystemHistoryService.CreateSnapshot();

            return GetDiff(updater.LastSyncSnapshotId, newSnapshot.Id);
        }

        IMultiFileSystemDiff GetDiff([CanBeNull] string fromId, [NotNull] string toId, string[] pathFilter = null)
        {
            if (toId == null)
                throw new ArgumentNullException(nameof(toId));

            return fromId == null
                ? m_MultiFileSystemHistoryService.GetChanges(toId, pathFilter)          // sync was never executed => get all changes
                : m_MultiFileSystemHistoryService.GetChanges(fromId, toId, pathFilter); // get the changes between the newly created snapshot and the last sync            
        }

        void Synchronize(IMultiFileSystemDiff diff, IMultiFileSystemChangeList changeList, ISyncStateUpdater updater)
        {
            using (m_Logger.BeginScope("Synchronize File"))
            {
                m_Logger.LogDebug($"Synchronizing {changeList.Path}");

                var conflict = updater.GetConflictOrDefault(changeList.Path);
                var actions = updater.GetUncompletedActions(changeList.Path);
                
                // if there are conflicts or uncompleted actions for the file
                // do not just look at the changes since the last sync
                // but the changes since the conflict or sync action was created
                if(conflict != null || actions.Count > 0)
                {
                    // find oldest snapshot from conflict or pending sync actions
                    var snapshotIds = actions.Select(x => x.SnapshotId).ToHashSet();
                    if (conflict != null)
                    {
                        snapshotIds.Add(conflict.SnapshotId);

                        // remove a conflict if it exists (file has changed so conflict might no longer exist)
                        // if there are still multiple versions, a new conflict will be created later                
                        m_Logger.LogDebug("Conflict exists for file. Removing existing conflict because file has changed");
                        updater.Remove(conflict);
                    }

                    var oldestSnapshot = snapshotIds
                        .Select(id => m_MultiFileSystemHistoryService[id])
                        .OrderBy(snapshot => snapshot.CreationTime)
                        .First();

                    // for this file, not the changes since the last sync but the changes since the conflict was created are relevant
                    // skip if the diff we already have covers the desired range
                    if(oldestSnapshot.CreationTime < diff.FromSnapshot.CreationTime)
                    {
                        m_Logger.LogDebug($"Including changes back to snapshot {oldestSnapshot.Id} because uncompleted sync actions or conflicts exist for the file");
                        diff = GetDiff(oldestSnapshot.Id, diff.ToSnapshot.Id, new[] { changeList.Path });
                        changeList = diff.FileChanges.Single();
                    }
                }

                // get the current versions of the files present in the individual histories
                var currentVersions = diff.ToSnapshot.GetFiles(changeList.Path);
                var currentFileReferences = currentVersions.Select(x => x.file?.ToReference()).ToHashSet(m_FileReferenceComparer);

                // remove uncompleted sync actions that are no longer applicable
                m_Logger.LogDebug("Removing uncompleted sync actions that are no longer applicable");
                foreach (var action in updater.GetUncompletedActions(changeList.Path))
                {
                    if(!currentFileReferences.Contains(action.FromVersion) || !currentFileReferences.Contains(action.ToVersion))
                    {
                        m_Logger.LogDebug($"Removing sync action '{action}'");
                        updater.Remove(action);
                    }
                }


                // build a graph from all the changes and get the sinks (nodes without outgoing edges)
                var graph = GetChangeGraph(changeList, updater);
                var sinks = graph.GetSinks();

                if(sinks.Count == 0)
                {
                    // no sinks => graph might contain a loop
                    // conflict needs to be resolved manually
                    m_Logger.LogInformation("Multiple version exist, creating conflict");
                    updater.AddConflict(changeList.Path, currentFileReferences);
                }
                else if (sinks.Count == 1)
                {
                    // only a single sink => select this file as new version
                    var selectedVersion = sinks.Single();
                    m_Logger.LogDebug($"Selected new version of file: {selectedVersion}");

                    // remove sync old actions
                    m_Logger.LogDebug("Removing sync actions not resulting in selected version");
                    foreach (var action in updater.GetUncompletedActions(changeList.Path))
                    {
                        if(!m_FileReferenceComparer.Equals(selectedVersion, action.ToVersion))
                        {
                            updater.Remove(action);
                        }
                    }

                    // add new actions to replace all version with the selected version
                    m_Logger.LogDebug("Creating new sync actions");
                    foreach (var item in currentVersions.Where(x => !m_FileReferenceComparer.Equals(x.file?.ToReference(), selectedVersion)))
                    {
                        updater.AddSyncAction(item.historyName, item.file?.ToReference(), selectedVersion);
                    }
                }
                else
                {
                    m_Logger.LogInformation("Multiple version exist, creating conflict");
                    updater.AddConflict(changeList.Path, sinks);
                }
            }
        }

        Graph<IFileReference, object> GetChangeGraph(IMultiFileSystemChangeList changeList, ISyncStateUpdater updater)
        {
            m_Logger.LogDebug("Building change graph");
            var graph = new Graph<IFileReference, object>(m_FileReferenceComparer);

            // add all changes to the graph
            foreach (var change in changeList.AllChanges)
            {
                graph.AddNode(change.FromVersion);
                graph.AddNode(change.ToVersion);
                graph.AddEdge(change.FromVersion, change.ToVersion, null);
            }

            // if there are uncompleted sync actions for the file, add them as well
            m_Logger.LogDebug("Processing pending actions");
            foreach (var action in updater.GetUncompletedActions(changeList.Path))
            {
                graph.AddNode(action.FromVersion);
                graph.AddNode(action.ToVersion);
                graph.AddEdge(action.FromVersion, action.ToVersion, null);                
            }

            return graph;
        }        
    }
}