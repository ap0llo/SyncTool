// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class RemoveFileSyncAction : SyncAction
    {     
        public RemoveFileSyncAction(Guid id, string target,SyncActionState state, int syncPointId, IFileReference removedFile) 
            : base(ChangeType.Deleted, removedFile, null, id, target, state, syncPointId)
        {
        
        }
        
    }
}