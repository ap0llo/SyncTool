// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.Conflicts;

namespace SyncTool.Synchronization
{
    public abstract class ConflictResolverBase
    {
        readonly ChangeGraphBuilder m_ChangeGraphBuilder;


        public ConflictResolverBase(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if (fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }

            m_ChangeGraphBuilder = new ChangeGraphBuilder(fileReferenceComparer);
        }


        public void ResolveConflicts(IGroup group)
        {
            var conflictService = group.GetSyncConflictService();
            var syncActionService = group.GetSyncActionService();
            var historyService = group.GetHistoryService();

            var syncStateUpdater = new SyncActionUpdateBuilder();

            foreach (var conflict in conflictService.Items)
            {
                IFileReference resolved;
                if (TryResolveConflict(historyService, conflict, out resolved))
                {
                    // remove the conflict
                    syncStateUpdater.RemoveConflict(conflict);


                    // add sync actions
                    throw new NotImplementedException();
                }
            }

            syncStateUpdater.Apply(syncActionService, conflictService);
        }


        protected abstract bool TryResolveConflict(IEnumerable<IFileReference> versions, out IFileReference resolvedVersion);


        bool TryResolveConflict(IHistoryService historyService, ConflictInfo conflict, out IFileReference resolved)
        {
            var graph = m_ChangeGraphBuilder.GetChangeGraphs(GetDiffs(historyService, conflict)).Single();

            var sinks = graph.GetSinks().ToArray();

            return TryResolveConflict(sinks, out resolved);
        }
        
        IEnumerable<IFileSystemDiff> GetDiffs(IHistoryService historyService, ConflictInfo conflict)
        {
            foreach (var folderName in conflict.SnapshotIds.Keys)
            {
                var snapshotId = conflict.SnapshotIds[folderName];
                var history = historyService[folderName];

                if (snapshotId == null)
                {
                    yield return history.GetChanges(history.LatestFileSystemSnapshot.Id, new[] { conflict.FilePath });
                }
                else
                {
                    yield return history.GetChanges(snapshotId, history.LatestFileSystemSnapshot.Id, new[] { conflict.FilePath });
                }
            }
        }
    }
}