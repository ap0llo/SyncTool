// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem
{
    internal static class DirectoryHelper
    {
        /// <summary>
        /// Gets the specified directory or creates it
        /// </summary>
        /// <remarks>Assumes that all <see cref="IDirectory"/> objects are instances of <see cref="Directory"/> </remarks>        
        /// <remarks>       
        /// This is intentionally not an extension method to prevent it from showing up in auto-completion, because this is not a general purpose method
        /// (makes assumption about the directory object that will not always be true)
        /// </remarks>
        internal static Directory GetOrAddDirectory(Directory parent, string relativePath)
        {
            if (relativePath == "")
            {
                return parent;
            }

            if (parent.DirectoryExists(relativePath))
            {
                return (Directory)parent.GetDirectory(relativePath);
            }
            else
            {
                var name = PathParser.GetFileName(relativePath);
                var directParent = GetOrAddDirectory(parent, PathParser.GetDirectoryName(relativePath));
                return (Directory)directParent.Add(d => new Directory(d, name));
            }
        }

    }
}