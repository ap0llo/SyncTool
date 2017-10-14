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

        
        bool TryApply();
        
        IReadOnlyCollection<ISyncAction> GetActions(string path);

        [CanBeNull]
        SyncConflict GetConflictOrDefault(string path);

        void Remove(ISyncAction syncAction);

        void Remove(SyncConflict conflict);

        void AddSyncAction(string historyName, FileReference fileReference, FileReference selectedVersion);

        void AddConflict(string path, IEnumerable<FileReference> currentFileReferences);
    }
}
