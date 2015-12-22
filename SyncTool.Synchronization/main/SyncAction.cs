// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public abstract class SyncAction
    {        
        public SyncActionType Type { get; }

        protected SyncAction(SyncActionType type)
        {
            this.Type = type;
        }

    }
}