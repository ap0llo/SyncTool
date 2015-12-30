// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    public static class FileSystemHistoryExtensions
    {

        public static IFileSystemSnapshot GetOldestSnapshot(this IFileSystemHistory history)
        {
            return history.Snapshots.OrderBy(x => x.CreationTime).First();
        }

    }
}