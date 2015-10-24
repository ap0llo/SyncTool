using System;
using System.IO;
using System.Text;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;
using Xunit;

namespace SyncTool.FileSystem.Test
{
    public class CreateDirectoryVisitorTest
    {

        private class TestReadableFile : IReadableFile
        {
            readonly string m_Content;

            public TestReadableFile(string name, string content)
            {
                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }
                this.m_Content = content;
                this.Name = name;
            }

            public string Name { get; }

            public DateTime LastWriteTime { get { throw new NotImplementedException();} }

            public long Length { get { throw  new NotImplementedException();} }

            public Stream Open(FileMode mode)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(m_Content));
            }
        }


        private const string s_Dir1 = "dir1";
        private const string s_Dir2 = "dir2";
        private const string s_File1 = "file1";

        readonly CreateLocalDirectoryVisitor m_Instance;

        public CreateDirectoryVisitorTest()
        {
            m_Instance = new CreateLocalDirectoryVisitor();
        }

        [Fact]
        public void CreateDirectory()
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
        public void CreateDirectory_Content_from_Readable_files_gets_written_to_disk()
        {
            var rootName = Path.GetRandomFileName();
            const string fileContent = "Hello World!";

            var directory = new Directory(rootName)
            {
               new TestReadableFile(s_File1, fileContent)
            };
            
            m_Instance.CreateDirectory(directory, Path.GetTempPath());

            var expectedFilePath = Path.Combine(Path.GetTempPath(), rootName, s_File1);

            Assert.True(NativeFile.Exists(expectedFilePath));
            Assert.Equal(fileContent, NativeFile.ReadAllText(expectedFilePath));
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