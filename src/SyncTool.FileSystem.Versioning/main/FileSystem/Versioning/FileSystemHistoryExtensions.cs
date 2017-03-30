// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Extension methods for <see cref="IFileSystemHistory"/>
    /// </summary>
    public static class FileSystemHistoryExtensions
    {
        /// <summary>
        /// Gets the oldest snapshot from the history
        /// </summary>        
        public static IFileSystemSnapshot GetOldestSnapshot(this IFileSystemHistory history)
        {
            return history.Snapshots.OrderBy(x => x.CreationTime).First();
        }

    }
}