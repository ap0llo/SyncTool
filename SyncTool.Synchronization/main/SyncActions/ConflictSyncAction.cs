// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public abstract class ConflictSyncAction : SyncAction
    {
        
        public string Description { get; set; }

    }
}