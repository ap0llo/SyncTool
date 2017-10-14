using System;
using System.Collections.Generic;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Synchronization.State;

namespace SyncTool.Sql.Services
{
    class SqlSyncAction : ISyncAction
    {
        readonly SyncStateRepository m_Repository;
        readonly SyncActionDo m_SyncActionDo;


        public string SnapshotId => m_SyncActionDo.SnapshotId;

        // either FromVersion or ToVersion is != null
        public string Path => m_SyncActionDo.FromVersion?.Path ?? m_SyncActionDo.ToVersion.Path;

        public FileReference FromVersion { get; }

        public FileReference ToVersion { get; }
        

        public SqlSyncAction(SyncStateRepository repository, SyncActionDo syncActionDo)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_SyncActionDo = syncActionDo ?? throw new ArgumentNullException(nameof(syncActionDo));

            // fully load data for this sync action
            repository.LoadVersions(syncActionDo);

            FromVersion = m_SyncActionDo.FromVersion?.ToFileReference();
            ToVersion = m_SyncActionDo.ToVersion?.ToFileReference();
        }

        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as ISyncAction);

        public bool Equals(ISyncAction other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.Ordinal.Equals(SnapshotId, other.SnapshotId) &&
                EqualityComparer<FileReference>.Default.Equals(FromVersion, other.FromVersion) &&
                EqualityComparer<FileReference>.Default.Equals(ToVersion, other.ToVersion);
        }
    }
}
