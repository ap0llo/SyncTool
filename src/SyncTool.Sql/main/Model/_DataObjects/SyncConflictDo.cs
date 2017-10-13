using System;
using System.Collections.Generic;
using SyncTool.Synchronization.State;

namespace SyncTool.Sql.Model
{
    public sealed class SyncConflictDo
    {
        public int Id { get; set; }

        public string SnapshotId { get; set; }

        public List<FileReferenceDo> ConflictingVersions { get; set; }


        public static SyncConflictDo FromSyncConflict(IConflict conflict)
        {
            throw new NotImplementedException();
        }

    }
}
