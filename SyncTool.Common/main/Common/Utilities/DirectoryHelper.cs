// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;

namespace SyncTool.Common.Utilities
{
    /// <summary>
    /// Utility class for features missing from <see cref="System.IO.Directory"/>
    /// </summary>
    public class DirectoryHelper
    {
        /// <summary>
        /// Deletes the specified directory recursively. If files within the directory are write-protected
        /// the protection will be removed and the will will be deleted anyways
        /// </summary>
        public static void DeleteRecursively(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);
            
            // remove read-only flag from all files before deleting    
            directoryInfo.Attributes = FileAttributes.Normal;

            foreach (var info in directoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directoryInfo.Delete(true);
        }
    }
}