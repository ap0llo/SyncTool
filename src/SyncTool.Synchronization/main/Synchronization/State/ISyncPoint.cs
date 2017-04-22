using System;
using System.Collections.Generic;
using SyncTool.Configuration.Model;

namespace SyncTool.Synchronization.State
{
    public interface ISyncPoint
    {
        /// <summary>
        /// Gets the id of the synchronization point
        /// </summary>
        int Id { get; }
                
        /// <summary>
        /// Gets the id of the latest multifilesystem snapshot that was included in the last sync
        /// </summary>
        string MultiFileSystemSnapshotId { get; }        
    }
}