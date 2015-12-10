// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;

namespace SyncTool.Utilities
{
    public class DirectoryHelper
    {
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