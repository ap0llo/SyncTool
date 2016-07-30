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
using SyncTool.Synchronization.ChangeGraph;
using SyncTool.Synchronization.Conflicts;

namespace SyncTool.Synchronization
{
    public abstract class ConflictResolverBase
    {
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
        

        public ConflictResolverBase(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            if (fileReferenceComparer == null)
            {
                throw new ArgumentNullException(nameof(fileReferenceComparer));
            }
            m_FileReferenceComparer = fileReferenceComparer;
            
        }


        public void ResolveConflicts(IGroup group)
        {
            var conflictService = group.GetSyncConflictService();
            var syncActionService = group.GetSyncActionService();
            var historyService = group.GetService<IMultiFileSystemHistoryService>();

            var changeGraphBuilder = new ChangeGraphBuilder(m_FileReferenceComparer);

            var syncStateUpdater = new SyncActionUpdateBuilder();

            foreach (var conflict in conflictService.Items)
            {
                IFileReference resolved;
                if (TryResolveConflict(changeGraphBuilder, historyService, conflict, out resolved))
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


        bool TryResolveConflict(ChangeGraphBuilder changeGraphBuilder, IMultiFileSystemHistoryService historyService, ConflictInfo conflict, out IFileReference resolved)
        {
            var graph = changeGraphBuilder.GetChangeGraphs(GetDiff(historyService, conflict)).Single();

            var sinks = graph.GetSinks().ToArray();

            return TryResolveConflict(sinks, out resolved);
        }
        
        IMultiFileSystemDiff GetDiff(IMultiFileSystemHistoryService historyService, ConflictInfo conflict)
        {
            return historyService.GetChanges(conflict.SnapshotId, historyService.LatestSnapshot.Id, new[] {conflict.FilePath});         
        }
    }
}