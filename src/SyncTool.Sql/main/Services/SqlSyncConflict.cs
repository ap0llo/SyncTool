using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Synchronization.State;
using SyncTool.Utilities;

namespace SyncTool.Sql.Services
{
    class SqlSyncConflict : ISyncConflict
    {
        readonly SyncStateRepository m_Repository;
        readonly SyncConflictDo m_SyncConflictDo;


        public string SnapshotId => m_SyncConflictDo.SnapshotId;

        public string Path => m_SyncConflictDo.ConflictingVersions.First(x => x != null).Path;
        
        public IReadOnlyList<IFileReference> ConflictingVersions { get; }


        public SqlSyncConflict(SyncStateRepository repository, SyncConflictDo syncConflictDo)
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_SyncConflictDo = syncConflictDo ?? throw new ArgumentNullException(nameof(syncConflictDo));

            m_Repository.LoadConflictingVersions(syncConflictDo);

            ConflictingVersions = m_SyncConflictDo.ConflictingVersions.Select(x => x?.ToSqlFileReference()).ToList();
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as ISyncConflict);

        public bool Equals(ISyncConflict other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.Ordinal.Equals(SnapshotId, other.SnapshotId) &&
                   ConflictingVersions.SetEqual(other.ConflictingVersions);
        }
    }
}
