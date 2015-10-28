using System;
using System.Linq;
using Moq;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class MetaFileSystemCreatorTest
    {
        const string s_File1 = "file1.txt";
        const string s_Dir1 = "dir1";

        readonly MetaFileSystemCreator m_Instance = new MetaFileSystemCreator();


        [Fact]
        public void CreateMetaDirectory_with_a_single_file()
        {
            var fileMock = CreateFileMock(s_File1, DateTime.Now, 1234);

            var directory = new Directory("root") { fileMock.Object };


            var metaFileSystem = m_Instance.CreateMetaDirectory(directory);

            Assert.Equal(0, metaFileSystem.Directories.Count());
            Assert.Equal(2, metaFileSystem.Files.Count());

            Assert.True(metaFileSystem.FileExists(s_File1 + FilePropertiesFile.FileNameSuffix));
            Assert.True(metaFileSystem.FileExists(DirectoryPropertiesFile.FileName));
        }

        [Fact]
        public void CreateMetaDirectory_with_directories_and_files()
        {
            var fileMock1 = CreateFileMock(s_File1, DateTime.Now, 1234);
            var fileMock2 = CreateFileMock(s_File1, DateTime.Now, 5678);

            var directory = new Directory("root")
            {
                fileMock1.Object,
                new Directory(s_Dir1)
                {
                    fileMock2.Object
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