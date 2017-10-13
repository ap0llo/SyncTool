using SyncTool.Synchronization.State;
using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Sql.Model
{
    public sealed class SyncActionDo
    {
        public int Id { get; set; }

        public string SnapshotId { get; set; }

        public FileReferenceDo FromVersion { get; set; }

        public FileReferenceDo ToVersion { get; set; }


        public static SyncActionDo FromSyncAction(ISyncAction syncAction)
        {
            throw new NotImplementedException();
        }
    }
}
