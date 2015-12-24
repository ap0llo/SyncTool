// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Synchronization
{
    public abstract class ResolvedSyncAction : SyncAction
    {
        public SyncParticipant Target { get; }

        protected ResolvedSyncAction(SyncParticipant target)
        {
            this.Target = target;
        }


    }
}