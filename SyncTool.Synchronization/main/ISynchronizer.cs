// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public interface ISynchronizer
    {
        IEnumerable<SyncAction> Synchronize(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges);
    }
}