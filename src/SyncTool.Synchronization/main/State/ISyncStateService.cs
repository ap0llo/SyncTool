using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Synchronization.State
{
    public interface ISyncStateService
    {
        string LastSyncSnapshotId { get; }    

        IReadOnlyCollection<ISyncAction> Actions { get; }

        IReadOnlyCollection<ISyncConflict> Conflicts { get; }
        

        ISyncStateUpdater BeginUpdate(string newLastSynapshotId);
    }
}
