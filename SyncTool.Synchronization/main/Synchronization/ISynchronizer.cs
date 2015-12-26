﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public interface ISynchronizer
    {
        ISynchronizerResult Synchronize(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges);
    }
}