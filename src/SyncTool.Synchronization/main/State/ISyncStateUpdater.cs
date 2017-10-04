using System;
using System.Collections.Generic;
using System.Text;
using SyncTool.FileSystem;
using JetBrains.Annotations;

namespace SyncTool.Synchronization.State
{
    public interface ISyncStateUpdater : IDisposable
    {
        string LastSyncSnapshotId { get; }


        IEnumerable<ISyncAction> AddedSyncActions { get; }
        IEnumerable<ISyncAction> RemovedSyncActions { get; }
        IEnumerable<IConflict> AddedConflicts { get; }
        IEnumerable<IConflict> RemovedConflicts { get; }

        bool TryApply();
        
        IReadOnlyCollection<ISyncAction> GetUncompletedActions(string path);

        [CanBeNull]
        IConflict GetConflictOrDefault(string path);

        void Remove(ISyncAction syncAction);

        void Remove(IConflict conflict);

        void AddSyncAction(string historyName, IFileReference fileReference, IFileReference selectedVersion);

        void AddConflict(string path, IEnumerable<IFileReference> currentFileReferences);
    }
}
