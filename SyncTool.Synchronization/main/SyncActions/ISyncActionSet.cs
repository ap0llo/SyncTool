﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public interface ISyncActionSet : IEnumerable<SyncAction>
    {
        void Add(SyncAction action);

        IDirectory ApplyTo(IDirectory directory);
    }
}