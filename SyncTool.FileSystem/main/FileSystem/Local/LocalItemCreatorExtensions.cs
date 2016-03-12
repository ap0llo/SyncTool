// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.IO;

namespace SyncTool.FileSystem.Local
{
    public static class LocalItemCreatorExtensions
    {
        /// <summary>
        /// Creates the specified directory structure as temporary directory
        /// </summary>
        /// <returns>Returns a <see cref="DisposableLocalDirectoryWrapper"/> for the created directory that will delete the directory when disposed</returns>
        public static DisposableLocalDirectoryWrapper CreateTemporaryDirectory(this LocalItemCreator visitor, IDirectory directory)
        {
            return visitor.CreateDirectory(directory, Path.GetTempPath()).ToTemporaryDirectory();
        }

        /// <summary>
        /// Creates an empty temporary directory
        /// </summary>
        /// <returns>Returns a <see cref="DisposableLocalDirectoryWrapper"/> for the created directory that will delete the directory when disposed</returns>
        public static DisposableLocalDirectoryWrapper CreateTemporaryDirectory(this LocalItemCreator visitor)
        {
            return visitor.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName()));
        }

        /// <summary>
        /// Creates all the children of the specified directory in the specified path 
        /// </summary>        
        public static void CreateDirectoryInPlace(this LocalItemCreator visitor, IDirectory directory, string createIn)
        {
            var name = Path.GetFileName(createIn.Trim("\\//".ToCharArray()));
            createIn = Path.GetDirectoryName(createIn);
            
            visitor.CreateDirectory(new Directory(name, directory.Directories, directory.Files), createIn);            
        }
    }
}