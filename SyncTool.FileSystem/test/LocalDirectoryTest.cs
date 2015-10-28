using System.IO;
using System.Linq;
using Xunit;

namespace SyncTool.FileSystem.Test
{
    public class LocalDirectoryTest
    {
        readonly CreateLocalDirectoryVisitor m_CreateLocalDirectoryVisitor = new CreateLocalDirectoryVisitor();



        [Fact]
        public void LoadFileSystem_from_flat_directory_containing_only_files()
        {
            var fileNames = new[] {"file1", "file2.ext", "file3"};

            var temporaryDirectory = m_CreateLocalDirectoryVisitor.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName(), fileNames.Select(name => new EmptyFile(name))));

            using (temporaryDirectory)
            {
                var localDirectory = new LocalDirectory(temporaryDirectory.Location);

                Assert.Equal(3, localDirectory.Files.Count());
                Assert.Equal(0, localDirectory.Directories.Count());

                foreach (var fileName in fileNames)
                {
                    Assert.True(localDirectory.FileExists(fileName));
                }
            }
        }

        [Fact]
        public void LoadFileSystem_from_flat_directory_containing_only_directories()
        {
            var dirNames = new[] {"dir1", "dir2", "dir3"};

            var temporaryDirectory = m_CreateLocalDirectoryVisitor.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName(), dirNames.Select(dir => new Directory(dir))));

            using (temporaryDirectory)
            {
                var localDirectory = new LocalDirectory(temporaryDirectory.Location);

                Assert.Equal(0, localDirectory.Files.Count());
                Assert.Equal(dirNames.Length, localDirectory.Directories.Count());

                foreach (var dirName in dirNames)
                {
                    Assert.True(localDirectory.DirectoryExists(dirName));
                    Assert.Empty(localDirectory.GetDirectory(dirName).Directories);
                    Assert.Empty(localDirectory.GetDirectory(dirName).Files);
                    Assert.Equal(localDirectory.GetDirectory(dirName), localDirectory[dirName]);
                }
            }
        }

        [Fact]
        public void LoadFileSystem_sets_Name_to_name_of_directory()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            dirInfo.Create();

            var localDirectory = new LocalDirectory(dirInfo.FullName);
            Assert.Equal(dirInfo.Name, localDirectory.Name);

            dirInfo.Delete(true);
        }
    }
}