// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Linq;

namespace SyncTool.Synchronization.Transfer
{
    public static class SynchronizationStateExtensions
    {

        public static bool IsCompleted(this ISynchronizationState state)
        {
            return state.QueuedActions.Any() || state.InProgressActions.Any();
        }

    }
}