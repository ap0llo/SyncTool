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


        public SyncConflict ToSyncConflict(SyncStateRepository repository)
        {
            repository.LoadConflictingVersions(this);
            return new SyncConflict(SnapshotId, ConflictingVersions.Select(x => x?.ToFileReference()));
        }


        public static SyncConflictDo FromSyncConflict(SyncConflict conflict) =>
            new SyncConflictDo()
            {
                SnapshotId = conflict.SnapshotId,
                ConflictingVersions = conflict.ConflictingVersions.Select(FileReferenceDo.FromFileReference).ToList()
            };


    }
}
