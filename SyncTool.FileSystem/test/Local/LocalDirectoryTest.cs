// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.IO;
using System.Linq;
using Xunit;
using NativeDirectory = System.IO.Directory;
using NativeFile = System.IO.File;

namespace SyncTool.FileSystem.Local
{
    public class LocalDirectoryTest
    {
        readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();



        [Fact]
        public void Flat_directory_containing_only_files()
        {
            var fileNames = new[] {"file1", "file2.ext", "file3"};

            var temporaryDirectory = m_LocalItemCreator.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName(), fileNames.Select(name => new EmptyFile(name))));

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
        public void Flat_directory_containing_only_directories()
        {
            var dirNames = new[] {"dir1", "dir2", "dir3"};

            var temporaryDirectory = m_LocalItemCreator.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName(), dirNames.Select(dir => new Directory(dir))));

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
        public void Name_is_set_to_name_of_directory()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            dirInfo.Create();

            var localDirectory = new LocalDirectory(dirInfo.FullName);
            Assert.Equal(dirInfo.Name, localDirectory.Name);

            dirInfo.Delete(true);
        }

        [Fact]
        public void Files_reflects_deletions_on_disk()
        {
            var fileNames = new[] { "file1", "file2.ext", "file3" };

            var temporaryDirectory = m_LocalItemCreator.CreateTemporaryDirectory(
                new Directory(Path.GetRandomFileName(), fileNames.Select(name => new EmptyFile(name)))
                );

            using (temporaryDirectory)
            {
                var localDirectory = new LocalDirectory(temporaryDirectory.Location);

                Assert.Equal(fileNames.Length, localDirectory.Files.Count());
                Assert.Equal(0, localDirectory.Directories.Count());
                
                NativeFile.Delete(Path.Combine(temporaryDirectory.Location, fileNames.First()));

                Assert.Equal(fileNames.Length - 1 , localDirectory.Files.Count());
            }
        }

        [Fact]
        public void Directories_reflects_deletions_on_disk()
        {
            var dirNames = new[] { "dir1", "dir2", "dir3" };

            var temporaryDirectory = m_LocalItemCreator.CreateTemporaryDirectory(new Directory(Path.GetRandomFileName(), dirNames.Select(dir => new Directory(dir))));

            using (temporaryDirectory)
            {
                var localDirectory = new LocalDirectory(temporaryDirectory.Location);

                Assert.Equal(0, localDirectory.Files.Count());
                Assert.Equal(dirNames.Length, localDirectory.Directories.Count());

                
                NativeDirectory.Delete(Path.Combine(temporaryDirectory.Location, dirNames.First()));

                Assert.Equal(0, localDirectory.Files.Count());
                Assert.Equal(dirNames.Length - 1, localDirectory.Directories.Count());


            }
        }
        
       
    }
}