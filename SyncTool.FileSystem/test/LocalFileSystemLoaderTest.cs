using System;
using System.Linq;
using System.Reflection;
using SyncTool.Utilities;
using Xunit;

namespace SyncTool.FileSystem.Test
{
    public class LocalFileSystemLoaderTest : IDisposable
    {
        readonly TemporaryDirectory m_TempDirectory;
        readonly LocalFileSystemLoader m_Instance;


        public LocalFileSystemLoaderTest()
        {
            m_TempDirectory = new TemporaryDirectory();
            m_Instance = new LocalFileSystemLoader(m_TempDirectory.Directory.FullName);
        }



        [Fact]
        public void T01_LoadFileSystem_from_flat_directory_containing_only_files()
        {
            var files  = m_TempDirectory.CreateFiles("file1", "file2.ext", "file3");
            
            var directory = m_Instance.LoadFileSystem();
                
            Assert.Equal(3, directory.Files.Count());
            Assert.Equal(0, directory.Directories.Count());

            foreach (var file in files)
            {
                Assert.Equal(directory[file.Name].Name, file.Name);
                Assert.Equal(directory[file.Name].Parent, directory);
            }

        }    

        [Fact]
        public void T02_LoadFileSystem_from_flat_directory_containing_only_directories()
        {
            var dirs = m_TempDirectory.CreateDirectories("dir1", "dir2", "foo").ToArray();

            var directory = m_Instance.LoadFileSystem();

            Assert.Equal(0, directory.Files.Count());
            Assert.Equal(dirs.Length, directory.Directories.Count());

            foreach (var dir in dirs)
            {
                Assert.Equal(directory[dir.Name].Name, dir.Name);
                Assert.Equal(directory[dir.Name].Parent, directory);
                Assert.Empty(((Directory) directory[dir.Name]).Directories);
                Assert.Empty(((Directory) directory[dir.Name]).Files);
            }
        }

        [Fact]
        public void T03_LoadFileSystem_sets_Name_to_name_of_directory()
        {
            var directory = m_Instance.LoadFileSystem();
            Assert.Equal(m_TempDirectory.Directory.Name, directory.Name);
        }

        [Fact]
        public void T04_LoadFileSystem_Parent_is_null()
        {
            var directory = m_Instance.LoadFileSystem();
            Assert.Null(directory.Parent);
        }
    
        public void Dispose()
        {
            m_TempDirectory.Dispose();
        }
    }
}