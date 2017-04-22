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