// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using Directory = SyncTool.FileSystem.Directory;

namespace SyncTool.Synchronization
{
    internal static class DirectoryExtensions
    {

        public static MutableDirectory ToMutableDirectory(this IDirectory directory)
        {
            return directory.ToMutableDirectory(null);
        }

        private static MutableDirectory ToMutableDirectory(this IDirectory directory, MutableDirectory parent)
        {
            var newDirectory = new MutableDirectory(directory.Name);
            foreach (var dir in directory.Directories)
            {
                newDirectory.Add(d => dir.ToMutableDirectory(d));
            }
            foreach (var file in directory.Files)
            {
                newDirectory.Add(d => file.WithParent(d));
            }

            return newDirectory;
        }


    }
}