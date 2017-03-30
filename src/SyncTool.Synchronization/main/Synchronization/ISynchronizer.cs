﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public interface ISynchronizer
    {
        void Synchronize(IGroup group);
    }
}