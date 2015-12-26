// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="MetaFileSystemToFileSystemConverter"/>
    /// </summary>
    public class MetaFileSystemToFileSystemConverterTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";

        readonly FileSystemToMetaFileSystemConverter m_FileSystemToMetaFileSystemConverter = new FileSystemToMetaFileSystemConverter();
        readonly MetaFileSystemToFileSystemConverter m_Instance = new MetaFileSystemToFileSystemConverter();


        [Fact(DisplayName = nameof(MetaFileSystemToFileSystemConverter) + ".Convert()")]
        public void Convert()
        {
            IFile file1 = null;
            IFile file2 = null;
            IFile file3 = null;
            IFile file4 = null;

            var expectedFileSystem = new Directory("root")
            {
                root => new Directory(root, s_Dir1)
                {
                    dir1 =>
                    {
                        file2 = new EmptyFile(dir1, "file2") {LastWriteTime = DateTime.Now, Length = 23456};
                        return file2;
                    },
                    dir1 =>
                    {
                        file3 = new EmptyFile(dir1, "file3") {LastWriteTime = DateTime.Now, Length = 789};
                        return file3;
                    }
                },
                root => new Directory(root, s_Dir2)
                {
                    dir2 =>
                    {
                        file4 = new EmptyFile(dir2, "file4") {LastWriteTime = DateTime.Now, Length = 1011};
                        return file4;
                    }
                },
                root =>
                {
                    file1 = new EmptyFile(root, "file1") {LastWriteTime = DateTime.Now, Length = 1234};
                    return file1;
                }                
            };

            var metaFileSystem = m_FileSystemToMetaFileSystemConverter.CreateMetaDirectory(expectedFileSystem);

            var convertedFileSystem = m_Instance.Convert(metaFileSystem).GetMappedDirectory(metaFileSystem);

            // check number of files and directories
            Assert.Equal(expectedFileSystem.Directories.Count(), convertedFileSystem.Directories.Count());
            Assert.Equal(expectedFileSystem.Files.Count(), convertedFileSystem.Files.Count());


            // compare files
            Assert.True(convertedFileSystem.FileExists(file1.Name));

            FileSystemAssert.FileEqual(file1, convertedFileSystem.GetFile(file1.Name));
            FileSystemAssert.FileEqual(file2, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file2.Name));
            FileSystemAssert.FileEqual(file3, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file3.Name));
            FileSystemAssert.FileEqual(file4, convertedFileSystem.GetDirectory(s_Dir2).GetFile(file4.Name));   
                               
        }


        [Fact(DisplayName = nameof(MetaFileSystemToFileSystemConverter) + "Convert(): Name in directory properties file overrides directory name")]
        public void Convert_name_in_directory_properties_file_overrides_directory_name()
        {            
            var metaFileSystem = new Directory("root")
            {
                root => new Directory(root, Path.GetRandomFileName())
                {
                    d => new DirectoryPropertiesFile(d, DateTime.Now, new DirectoryProperties() { Name = s_Dir2})
                },
                root => new DirectoryPropertiesFile(root, DateTime.Now, new DirectoryProperties() { Name = s_Dir1})
            };

            var convertedFileSystem = m_Instance.Convert(metaFileSystem).GetMappedDirectory(metaFileSystem);
            Assert.Empty(convertedFileSystem.Files);
            Assert.Equal(s_Dir1, convertedFileSystem.Name);
            Assert.Equal(1, convertedFileSystem.Directories.Count());
            Assert.Equal(s_Dir2, convertedFileSystem.Directories.Single().Name);
        }

    }
}