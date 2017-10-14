using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Synchronization.State
{
    public interface ISyncStateService
    {
        string LastSyncSnapshotId { get; }    

        IReadOnlyCollection<SyncAction> Actions { get; }

        IReadOnlyCollection<SyncConflict> Conflicts { get; }
        

        ISyncStateUpdater BeginUpdate(string newLastSynapshotId);
    }
}
