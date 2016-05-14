// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public sealed class AddFileSyncAction : SyncAction
    {      
        public AddFileSyncAction(Guid id, string target, SyncActionState state, int syncPointId, IFileReference toVersion) 
            : base(ChangeType.Added,null, toVersion, id, target, state, syncPointId)
        {
        }


        
    }
}