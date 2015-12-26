// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using SyncTool.FileSystem;
using Xunit;

namespace SyncTool.TestHelpers
{
    /// <summary>
    /// Provides assertions for SyncTool.FileSystem types 
    /// </summary>
    public static class FileSystemAssert
    {


        public static void DirectoryEqual(IDirectory expected, IDirectory actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Directories.Count(), actual.Directories.Count());
            Assert.Equal(expected.Files.Count(), actual.Files.Count());

            foreach (var directory in expected.Directories)
            {
                Assert.True(actual.DirectoryExists(directory.Name));
                DirectoryEqual(directory, actual.GetDirectory(directory.Name));
            }

            foreach (var file in expected.Files)
            {
                Assert.True(actual.FileExists(file.Name));
                FileEqual(file, actual.GetFile(file.Name));
            }
        }


        public static void FileEqual(IFile expected, IFile actual)
        {
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Length, actual.Length);
            Assert.Equal(expected.LastWriteTime, actual.LastWriteTime);
        }

    }
}