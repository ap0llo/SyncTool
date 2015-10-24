using System.IO;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;
using Xunit;

namespace SyncTool.FileSystem.Test
{
    public class CreateDirectoryVisitorTest
    {
        private const string s_Dir1 = "dir1";
        private const string s_Dir2 = "dir2";
        private const string s_File1 = "file1";

        readonly CreateLocalDirectoryVisitor m_Instance;

        public CreateDirectoryVisitorTest()
        {
            m_Instance = new CreateLocalDirectoryVisitor();
        }

        [Fact]
        public void Create_Directory()
        {
            var rootName = Path.GetRandomFileName();

            var directory = new Directory(rootName)
            {
                new Directory(s_Dir1)
                {
                    new EmptyFile(s_File1)
                },
                new Directory(s_Dir2)
            };
            
                     
            m_Instance.CreateDirectory(directory, Path.GetTempPath());                  
            
            Assert.True(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), rootName)));
            Assert.True(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), rootName, s_Dir1)));
            Assert.True(NativeFile.Exists(Path.Combine(Path.GetTempPath(), rootName, s_Dir1, s_File1)));
            Assert.True(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), rootName, s_Dir2)));
        }


        [Fact]
        public void CreateTemporaryDirectory_created_directory_gets_deleted_on_dispose()
        {
            var dirName = Path.GetRandomFileName();

            var directory = new Directory(dirName);
            
            var createdDir = m_Instance.CreateTemporaryDirectory(directory);

            Assert.True(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), dirName)));

            createdDir.Dispose();

            Assert.False(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), dirName)));
        }

    }
}