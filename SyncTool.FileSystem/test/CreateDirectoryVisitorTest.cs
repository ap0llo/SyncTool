using System;
using System.IO;
using System.Linq;
using System.Text;
using Xunit;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.FileSystem.Test
{
    public class CreateDirectoryVisitorTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";
        const string s_File1 = "file1";

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
            // create temporary directory
            var dirName = Path.GetRandomFileName();
            var directory = new Directory(dirName);

            var createdDir = m_Instance.CreateTemporaryDirectory(directory);

            // assert that the directory was really created
            Assert.True(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), dirName)));

            // dispose the temporary directory
            createdDir.Dispose();

            // directory has to be gone now
            Assert.False(NativeDirectory.Exists(Path.Combine(Path.GetTempPath(), dirName)));
        }


        [Fact]
        public void CreateDirectoryInPlace()
        {
            var directory = new Directory(Path.GetRandomFileName())
            {
                new Directory(s_Dir1),
                new Directory(s_Dir2),
                new EmptyFile(s_File1)
            };

            using (var temporaryDirectory = m_Instance.CreateTemporaryDirectory())
            {                
                m_Instance.CreateDirectoryInPlace(directory, temporaryDirectory.Location);
                
                Assert.NotEqual(temporaryDirectory.Name, directory.Name);
                Assert.Equal(directory.Directories.Count(), temporaryDirectory.Directories.Count());
                Assert.True(directory.DirectoryExists(s_Dir1));
                Assert.True(directory.DirectoryExists(s_Dir2));
                Assert.Equal(directory.Files.Count(), temporaryDirectory.Files.Count());
                Assert.True(directory.FileExists(s_File1));
            }
        }

        /// <summary>
        ///     Implementation of <see cref="IReadableFile" /> used for this test
        /// </summary>
        class TestReadableFile : IReadableFile
        {
            readonly string m_Content;

            public string Name { get; }

            public DateTime LastWriteTime
            {
                get { throw new NotImplementedException(); }
            }

            public long Length
            {
                get { throw new NotImplementedException(); }
            }

            public TestReadableFile(string name, string content)
            {
                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }
                m_Content = content;
                Name = name;
            }

            public Stream Open(FileMode mode)
            {
                return new MemoryStream(Encoding.UTF8.GetBytes(m_Content));
            }
        }
    }
}