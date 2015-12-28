// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization.Transfer
{
    /// <summary>
    /// Interface encapsulation management and retrival of SyncActions for a single synchronized folder
    /// </summary>
    public interface ISynchronizationState
    {         
        string LocalSnapshotId { get; }

        string GlobalSnapshotId { get; }

        IEnumerable<SyncAction> QueuedActions { get; }

        IEnumerable<SyncAction> InProgressActions { get; } 

        IEnumerable<SyncAction> CompletedActions { get; }         

    }
}