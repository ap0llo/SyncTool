using SyncTool.Synchronization.State;
using System;
using System.Collections.Generic;
using System.Text;
using SyncTool.FileSystem;

namespace SyncTool.Sql.Services
{
    public class SqlSyncStateUpdater : ISyncStateUpdater
    {
        public string LastSyncSnapshotId => throw new NotImplementedException();

        public IEnumerable<ISyncAction> AddedSyncActions => throw new NotImplementedException();

        public IEnumerable<ISyncAction> RemovedSyncActions => throw new NotImplementedException();

        public IEnumerable<IConflict> AddedConflicts => throw new NotImplementedException();

        public IEnumerable<IConflict> RemovedConflicts => throw new NotImplementedException();

        public void AddConflict(string path, IEnumerable<IFileReference> currentFileReferences)
        {
            throw new NotImplementedException();
        }

        public void AddSyncAction(string historyName, IFileReference fileReference, IFileReference selectedVersion)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public IConflict GetConflictOrDefault(string path)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<ISyncAction> GetUncompletedActions(string path)
        {
            throw new NotImplementedException();
        }

        public void Remove(ISyncAction syncAction)
        {
            throw new NotImplementedException();
        }

        public void Remove(IConflict conflict)
        {
            throw new NotImplementedException();
        }

        public bool TryApply()
        {
            throw new NotImplementedException();
        }
    }
}
