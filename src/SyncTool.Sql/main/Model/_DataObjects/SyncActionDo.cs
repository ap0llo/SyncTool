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


        public SyncAction ToSyncAction(SyncStateRepository repository)
        {
            repository.LoadVersions(this);
            return new SyncAction(SnapshotId, FromVersion.ToFileReference(), ToVersion.ToFileReference());
        }

        public static SyncActionDo FromSyncAction(SyncAction syncAction) =>
            new SyncActionDo()
            {
                SnapshotId = syncAction.SnapshotId,
                FromVersion = FileReferenceDo.FromFileReference(syncAction.FromVersion),
                ToVersion = FileReferenceDo.FromFileReference(syncAction.ToVersion)
            };
    }
}
