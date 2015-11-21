// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------
namespace SyncTool.FileSystem.Filtering
{
    public interface IFileSystemFilter
    {
        /// <summary>
        /// Checks if the filter applies to specified <see cref="IFileSystemItem"/> 
        /// </summary>
        /// <param name="item">The filesystem item to check</param>
        /// <returns>Returns true if the specified item is excluded by the filter</returns>
        bool Applies(IFileSystemItem item);

    }
}