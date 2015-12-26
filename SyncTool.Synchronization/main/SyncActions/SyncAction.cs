// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public abstract class SyncAction
    {        
        public abstract string FilePath { get; }

        public SyncParticipant Target { get; }

        protected SyncAction(SyncParticipant target)
        {
            this.Target = target;
        }


        public abstract void Accept<T>(ISyncActionVisitor<T> visitor, T parameter);        
    }
}