using System.Collections.Generic;
using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization.State
{
    public class MutableSyncPoint : ISyncPoint
    {
        public int Id { get; set; }
        
        public string MultiFileSystemSnapshotId { get; set; }        
    }
}