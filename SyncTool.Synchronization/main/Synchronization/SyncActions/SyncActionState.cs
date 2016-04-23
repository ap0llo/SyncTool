// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Synchronization.SyncActions
{
    public enum SyncActionState
    {
        /*         
            Queued ---> Active --->  Completed   
               |          |               |
               |          |               |
               |          |               v
               +-------------------> Cancelled
         */

        Queued,
        Active,
        Completed,
        Cancelled
    }


    public static class SyncActionStateExtensions
    {
        public static bool IsPendingState(this SyncActionState state)
        {
            switch (state)
            {
                case SyncActionState.Queued:                    
                case SyncActionState.Active:
                    return true;

                case SyncActionState.Completed:
                case SyncActionState.Cancelled:
                    return false;
                                        
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}