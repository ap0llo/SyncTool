// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using Moq;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="FileSystemToMetaFileSystemConverter"/>
    /// </summary>
    public class FileSystemToMetaFileSystemConverterTest
    {
        const string s_File1 = "file1.txt";
        const string s_Dir1 = "dir1";

        readonly FileSystemToMetaFileSystemConverter m_Instance = new FileSystemToMetaFileSystemConverter();


        [Fact(DisplayName = "FileSystemToMetaFileSystemConverter.CreateMetaDirectory() with a single file")]
        public void CreateMetaDirectory_with_a_single_file()
        {
            var fileMock = CreateFileMock(s_File1, DateTime.Now, 1234);            

            var directory = new Directory(null, "root") { root => fileMock.Object };
            fileMock.Setup(f => f.Parent).Returns(directory);

            var metaFileSystem = m_Instance.CreateMetaDirectory(directory);

            Assert.Equal(0, metaFileSystem.Directories.Count());
            Assert.Equal(2, metaFileSystem.Files.Count());

            Assert.True(metaFileSystem.FileExists(s_File1 + FilePropertiesFile.FileNameSuffix));
            Assert.True(metaFileSystem.FileExists(DirectoryPropertiesFile.FileName));
        }

        [Fact(DisplayName = "FileSystemToMetaFileSystemConverter.CreateMetaDirectory() with directories and files")]
        public void CreateMetaDirectory_with_directories_and_files()
        {
            var fileMock1 = CreateFileMock(s_File1, DateTime.Now, 1234);
            var fileMock2 = CreateFileMock(s_File1, DateTime.Now, 5678);

            var directory = new Directory("root")
            {
                root =>
                {
                    fileMock1.Setup(f => f.Parent).Returns(root);
                    return fileMock1.Object;
                },
                root => new Directory(root, s_Dir1)
                {
                    dir1 =>
                    {
                        fileMock2.Setup(f => f.Parent).Returns(dir1);
                        return fileMock2.Object;
                    }
                }
            };


            var metaFileSystem = m_Instance.CreateMetaDirectory(directory);

            Assert.Equal(1, metaFileSystem.Directories.Count());
            Assert.Equal(1 + 1, metaFileSystem.Files.Count());

            Assert.True(metaFileSystem.FileExists(fileMock1.Object.Name + FilePropertiesFile.FileNameSuffix));
            Assert.True(metaFileSystem.FileExists(DirectoryPropertiesFile.FileName));

            Assert.True(metaFileSystem.DirectoryExists(s_Dir1));
            var metaDir1 = metaFileSystem.GetDirectory(s_Dir1);

            Assert.Empty(metaDir1.Directories);
            Assert.Equal(2, metaDir1.Files.Count());

            Assert.True(metaDir1.FileExists(fileMock2.Object.Name + FilePropertiesFile.FileNameSuffix));
            Assert.True(metaDir1.FileExists(DirectoryPropertiesFile.FileName));
        }


        
        Mock<IFile> CreateFileMock(string name, DateTime lastWriteTime, long length)
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(m => m.Name).Returns(name);
            fileMock.Setup(m => m.LastWriteTime).Returns(lastWriteTime);
            fileMock.Setup(m => m.Length).Returns(length);

            return fileMock;
        }
        
    }
}