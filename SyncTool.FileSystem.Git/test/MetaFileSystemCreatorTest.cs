using System;
using System.Linq;
using Moq;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class MetaFileSystemCreatorTest
    {
        const string s_File1 = "file1.txt";

        readonly MetaFileSystemCreator m_Instance = new MetaFileSystemCreator();

        [Fact]
        public void CreateMetaDirectory_with_a_single_file()
        {            
            var fileMock = new Mock<IFile>();
            fileMock.Setup(m => m.Name).Returns(s_File1);
            fileMock.Setup(m => m.LastWriteTime).Returns(DateTime.Now);
            fileMock.Setup(m => m.Length).Returns(1234);

            var directory = new Directory("root") { fileMock.Object };


            var metaFileSystem = m_Instance.CreateMetaDirectory(directory);

            Assert.Equal(0, metaFileSystem.Directories.Count());
            Assert.Equal(1, metaFileSystem.Files.Count());

            Assert.True(metaFileSystem.FileExists(s_File1 + FilePropertiesFile.FileNameSuffix));            
        }



    }
}