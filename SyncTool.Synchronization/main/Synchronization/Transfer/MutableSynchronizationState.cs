// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization.Transfer
{
    public class MutableSynchronizationState : ISynchronizationState
    {
        public string LocalSnapshotId { get; set; }

        public string GlobalSnapshotId { get; set; }

        public IEnumerable<SyncAction> QueuedActions { get; set; } = Enumerable.Empty<SyncAction>();

        public IEnumerable<SyncAction> InProgressActions { get; set; } = Enumerable.Empty<SyncAction>();

        public IEnumerable<SyncAction> CompletedActions { get; set; } = Enumerable.Empty<SyncAction>();



    }
}