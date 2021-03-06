﻿using System.IO;

namespace SyncTool.Utilities
{
    /// <summary>
    /// Utility class for features missing from <see cref="System.IO.Directory"/>
    /// </summary>
    public static class DirectoryHelper
    {
        /// <summary>
        /// Deletes the specified directory recursively if it exists. If files within the directory are write-protected
        /// the protection will be removed and the will will be deleted anyways
        /// </summary>
        public static void DeleteRecursively(string directoryPath)
        {
            // if the directory dows not exist, we can return right away
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(directoryPath);
            
            // remove read-only flag from all files before deleting    
            directoryInfo.Attributes = FileAttributes.Normal;

            foreach (var info in directoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directoryInfo.Delete(true);
        }


        /// <summary>
        /// Removes all items from the specified directory recursively if it exists. If files within the directory are write-protected
        /// the protection will be removed and the will will be deleted anyways
        /// </summary>
        public static void ClearRecursively(string directoryPath)
        {
            // if the directory dows not exist, we can return right away
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var directoryInfo = new DirectoryInfo(directoryPath);                   
            foreach (var dir in directoryInfo.GetDirectories())
            {
                DeleteRecursively(dir.FullName);
            }

            foreach (var info in directoryInfo.GetFiles())
            {
                info.Attributes = FileAttributes.Normal;
                info.Delete();
            }

        }
    }
}