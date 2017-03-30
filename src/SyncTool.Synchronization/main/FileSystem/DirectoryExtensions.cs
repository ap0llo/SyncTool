// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using Directory = SyncTool.FileSystem.Directory;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Assembly-internal extension methods for <see cref="IDirectory"/>
    /// </summary>
    internal static class DirectoryExtensions
    {
        /// <summary>
        /// Gets a file reference for the file with the specified path from the directory
        /// or null if the file does not exist
        /// </summary>
        public static IFileReference GetFileReferenceOrDefault(this IDirectory directory, string path)
        {
            return directory.GetFileOrDefault(path)?.ToReference();
        }

        /// <summary>
        /// Creates a new mutable directory from the specified directory
        /// </summary>
        public static MutableDirectory ToMutableDirectory(this IDirectory directory)
        {
            return directory.ToMutableDirectory(null);
        }

        private static MutableDirectory ToMutableDirectory(this IDirectory directory, MutableDirectory parent)
        {
            var newDirectory = new MutableDirectory(parent, directory.Name);
            
            // recursively create mutable copies of all directories
            foreach (var dir in directory.Directories)
            {
                newDirectory.Add(d => dir.ToMutableDirectory(d));
            }

            // create copies of all file in the mutable directory
            foreach (var file in directory.Files)
            {
                newDirectory.Add(d => file.WithParent(d));
            }

            return newDirectory;
        }


    }
}