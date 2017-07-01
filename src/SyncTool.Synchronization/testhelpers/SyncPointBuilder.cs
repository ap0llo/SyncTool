using SyncTool.Synchronization.State;

namespace SyncTool.Synchronization.TestHelpers
{
    public static class SyncPointBuilder
    {
        public static MutableSyncPoint NewSyncPoint() => new MutableSyncPoint();

        public static MutableSyncPoint WithId(this MutableSyncPoint state, int id)
        {
            state.Id = id;
            return state;
        }

        public static MutableSyncPoint WithMultiFileSystemSnapshotId(this MutableSyncPoint state, string id)
        {            
            state.MultiFileSystemSnapshotId = id;            
            return state;
        }
    }
}
