﻿using System.IO;
using NativeDirectory = System.IO.Directory;

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
        /// <param name="visitor"></param>
        /// <param name="directory">The directory to create</param>
        /// <param name="createIn">The filesystem path to create the directory in</param>
        /// <param name="deleteExisting">
        /// If set to true, and the specified path already exists,
        /// all files currently at that path will be deleted before the directory is created in the specified location
        /// </param>
        public static void CreateDirectoryInPlace(this LocalItemCreator visitor, IDirectory directory, string createIn, bool deleteExisting = false)
        {
            if (deleteExisting && NativeDirectory.Exists(createIn))
            {
                NativeDirectory.Delete(createIn, true);
            }

            var name = Path.GetFileName(createIn.Trim("\\//".ToCharArray()));
            createIn = Path.GetDirectoryName(createIn);

            visitor.CreateDirectory(new Directory(name, directory.Directories, directory.Files), createIn);            
        }
    }
}