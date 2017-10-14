using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Synchronization.State;

namespace SyncTool.Sql.Model
{
    public sealed class SyncConflictDo
    {
        public int Id { get; set; }

        public string SnapshotId { get; set; }

        public List<FileReferenceDo> ConflictingVersions { get; set; }


        public static SyncConflictDo FromSyncConflict(ISyncConflict conflict) =>
            new SyncConflictDo()
            {
                SnapshotId = conflict.SnapshotId,
                ConflictingVersions = conflict.ConflictingVersions.Select(FileReferenceDo.FromFileReference).ToList()
            };

    }
}
