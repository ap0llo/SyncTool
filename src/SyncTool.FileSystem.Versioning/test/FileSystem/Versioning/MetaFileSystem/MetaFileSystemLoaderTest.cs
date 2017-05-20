using System.IO;
using Moq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using Xunit;
using Directory = SyncTool.FileSystem.Directory;

namespace SyncTool.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="MetaFileSystemLoader"/>
    /// </summary>
    public class MetaFileSystemLoaderTest
    {
        const string s_File1 = "file1";
        
        readonly MetaFileSystemLoader m_Instance = new MetaFileSystemLoader();


        [Fact]
        public void Convert_replaces_files_with_FilePropertiesFile_instances()
        {
            var input = new Directory(Path.GetRandomFileName())
            {
                dir => FilePropertiesFile.ForFile(dir, new EmptyFile(s_File1))
            };

            var directoryCreator = new LocalItemCreator();
            using (var temporaryDirectory = directoryCreator.CreateTemporaryDirectory(input))
            {
                var metaFs = m_Instance.Convert(temporaryDirectory.Directory);

                Assert.Empty(metaFs.Directories);
                Assert.True(metaFs.GetFile(s_File1 + FilePropertiesFile.FileNameSuffix) is FilePropertiesFile);
            }
        }

        [Fact]
        public void Convert_detection_of_file_properties_files_is_case_invariant()
        {
            var file1 = new EmptyFile(s_File1);
            var filePropertiesFile = FilePropertiesFile.ForFile(null, file1);

            var mock = new Mock<IReadableFile>(MockBehavior.Strict);
            mock.Setup(f => f.Name).Returns(file1.Name + FilePropertiesFile.FileNameSuffix.ToUpper());
            mock.Setup(f => f.OpenRead()).Returns(filePropertiesFile.OpenRead());

            var directoryCreator = new LocalItemCreator();
            using (var temporaryDirectory = directoryCreator.CreateTemporaryDirectory(
                new Directory(Path.GetRandomFileName())
                {
                    dir =>
                    {
                        mock.Setup(d => d.Parent).Returns(dir);
                        return mock.Object;
                    }
                }))
            {
                var metaFs = m_Instance.Convert(temporaryDirectory.Directory);

                Assert.Empty(metaFs.Directories);
                Assert.True(metaFs.GetFile(s_File1 + FilePropertiesFile.FileNameSuffix) is FilePropertiesFile);
            }

        }

        [Fact]
        public void Convert_replaces_files_with_DirectoryPropertiesFile_instances()
        {
            var input = new Directory(Path.GetRandomFileName());
            input.Add(_ => DirectoryPropertiesFile.ForDirectory(null, input));


            var directoryCreator = new LocalItemCreator();
            using (var temporaryDirectory = directoryCreator.CreateTemporaryDirectory(input))
            {
                var metaFs = m_Instance.Convert(temporaryDirectory.Directory);

                Assert.Empty(metaFs.Directories);
                Assert.True(metaFs.GetFile(DirectoryPropertiesFile.FileName) is DirectoryPropertiesFile);
            }
        }

        [Fact]
        public void Convert_detection_of_directory_properties_files_is_case_invariant()
        {
            var directory = new Directory(Path.GetRandomFileName());
            var directoryPropertiesFile = DirectoryPropertiesFile.ForDirectory(null, directory);
            
            var mock = new Mock<IReadableFile>();
            mock.Setup(f => f.Name).Returns(DirectoryPropertiesFile.FileName.ToUpper());
            mock.Setup(f => f.OpenRead()).Returns(directoryPropertiesFile.OpenRead());

            directory.Add(dir => mock.Object);

            var directoryCreator = new LocalItemCreator();
            using (var temporaryDirectory = directoryCreator.CreateTemporaryDirectory(directory))
            {
                var metaFs = m_Instance.Convert(temporaryDirectory.Directory);

                Assert.Empty(metaFs.Directories);
                Assert.True(metaFs.GetFile(DirectoryPropertiesFile.FileName) is DirectoryPropertiesFile);
            }

        }

    }
}