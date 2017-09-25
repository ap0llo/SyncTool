using System;
using System.IO;
using System.Linq;
using System.Text;
using SyncTool.FileSystem.Local;
using Xunit;
using NodaTime;

using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.FileSystem.Test.Local
{
    public class LocalItemCreatorTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";
        const string s_File1 = "file1";

        readonly LocalItemCreator m_Instance;


        public LocalItemCreatorTest()
        {
            m_Instance = new LocalItemCreator();
        }


        [Fact]
        public void CreateDirectory()
        {
            var rootName = Path.GetRandomFileName();

            var directory = new Directory(rootName)
            {
                root => new Directory(root, s_Dir1)
                {
                    dir1 => new EmptyFile(dir1, s_File1)
                },
                root => new Directory(root, s_Dir2)
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
                root => new TestReadableFile(root, s_File1, fileContent)
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
                d => new Directory(d, s_Dir1),
                d => new Directory(d, s_Dir2),
                d => new EmptyFile(d, s_File1)
            };

            using (var temporaryDirectory = m_Instance.CreateTemporaryDirectory())
            {                
                m_Instance.CreateDirectoryInPlace(directory, temporaryDirectory.Directory.Location);
                
                Assert.NotEqual(temporaryDirectory.Directory.Name, directory.Name);
                Assert.Equal(directory.Directories.Count(), temporaryDirectory.Directory.Directories.Count());
                Assert.True(directory.DirectoryExists(s_Dir1));
                Assert.True(directory.DirectoryExists(s_Dir2));
                Assert.Equal(directory.Files.Count(), temporaryDirectory.Directory.Files.Count());
                Assert.True(directory.FileExists(s_File1));
            }
        }
        

        /// <summary>
        ///     Implementation of <see cref="IReadableFile" /> used for this test
        /// </summary>
        class TestReadableFile : FileSystemItem, IReadableFile
        {
            readonly string m_Content;


            public Instant LastWriteTime => throw new NotImplementedException();

            public long Length => throw new NotImplementedException();


            public TestReadableFile(IDirectory parent, string name, string content) : base(parent, name)
            {
                m_Content = content ?? throw new ArgumentNullException(nameof(content));                
            }


            public Stream OpenRead() => new MemoryStream(Encoding.UTF8.GetBytes(m_Content));

            public IFile WithParent(IDirectory newParent) => throw new NotImplementedException();
        }
    }
}