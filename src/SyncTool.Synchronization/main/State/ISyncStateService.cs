using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Synchronization.State
{
    public interface ISyncStateService
    {
        string LastSyncSnapshotId { get; }    

        IEnumerable<ISyncAction> SyncActions { get; }

        IEnumerable<IConflict> Conflicts { get; }
        

        ISyncStateUpdater BeginUpdate();
    }
}
