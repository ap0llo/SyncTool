using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;

namespace SyncTool.FileSystem.Test
{
    public class LocalDirectoryTest 
    {
        readonly CreateLocalDirectoryVisitor m_CreateLocalDirectoryVisitor = new CreateLocalDirectoryVisitor();



        


        [Fact]
        public void T01_LoadFileSystem_from_flat_directory_containing_only_files()
        {
            var fileNames = new[] { "file1", "file2.ext", "file3"};

            var directory = m_CreateLocalDirectoryVisitor.CreateTemporaryDirectory(
                new Directory(Path.GetRandomFileName(), fileNames.Select(name => new EmptyFile(name))));

            using (directory)
            {
                Assert.Equal(3, directory.Files.Count());
                Assert.Equal(0, directory.Directories.Count());

                foreach (var fileName in fileNames)
                {
                    Assert.True(directory.FileExists(fileName));    
                }                
            }


        }    

        [Fact]
        public void T02_LoadFileSystem_from_flat_directory_containing_only_directories()
        {
            var dirNames = new[] {"dir1", "dir2", "dir3"};

            var directory = m_CreateLocalDirectoryVisitor.CreateTemporaryDirectory(
                new Directory(Path.GetRandomFileName(), dirNames.Select(dir => new Directory(dir))));


            using (directory)
            {
                Assert.Equal(0, directory.Files.Count());
                Assert.Equal(dirNames.Length, directory.Directories.Count());

                foreach (var dirName in dirNames)
                {
                    Assert.True(directory.DirectoryExists(dirName));
                    Assert.Empty(directory.GetDirectory(dirName).Directories);                
                    Assert.Empty(directory.GetDirectory(dirName).Files);                
                    Assert.Equal(directory.GetDirectory(dirName), directory[dirName]);
                }
                
            }
        }

        [Fact]
        public void T03_LoadFileSystem_sets_Name_to_name_of_directory()
        {
            var dirInfo = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            dirInfo.Create();

            var directory = new LocalDirectory(dirInfo.FullName);
            Assert.Equal(dirInfo.Name, directory.Name);

            dirInfo.Delete(true);
        }
        
    }
}