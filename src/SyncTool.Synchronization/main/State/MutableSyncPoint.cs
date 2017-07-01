using System.Collections.Generic;

namespace SyncTool.Synchronization.State
{
    public class MutableSyncPoint : ISyncPoint
    {
        public int Id { get; set; }
        
        public string MultiFileSystemSnapshotId { get; set; }        
    }
}