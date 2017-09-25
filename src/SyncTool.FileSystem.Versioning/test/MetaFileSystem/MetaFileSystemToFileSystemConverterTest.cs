using System;
using System.Linq;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.FileSystem.Versioning.MetaFileSystem;
using Xunit;
using NodaTime;

namespace SyncTool.FileSystem.Versioning.Test.MetaFileSystem
{
    /// <summary>
    /// Tests for <see cref="MetaFileSystemToFileSystemConverter"/>
    /// </summary>
    public class MetaFileSystemToFileSystemConverterTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";

        readonly FileSystemToMetaFileSystemConverter m_FileSystemToMetaFileSystemConverter = new FileSystemToMetaFileSystemConverter();
        readonly MetaFileSystemToFileSystemConverter m_Instance = new MetaFileSystemToFileSystemConverter();


        [Fact]
        public void Convert()
        {
            IFile file1 = null;
            IFile file2 = null;
            IFile file3 = null;
            IFile file4 = null;

            var expectedFileSystem = new Directory("root")
            {
                root => new Directory(root, s_Dir1)
                {
                    dir1 =>
                    {
                        file2 = new EmptyFile(dir1, "file2") {LastWriteTime = SystemClock.Instance.GetCurrentInstant(), Length = 23456};
                        return file2;
                    },
                    dir1 =>
                    {
                        file3 = new EmptyFile(dir1, "file3") {LastWriteTime = SystemClock.Instance.GetCurrentInstant(), Length = 789};
                        return file3;
                    }
                },
                root => new Directory(root, s_Dir2)
                {
                    dir2 =>
                    {
                        file4 = new EmptyFile(dir2, "file4") {LastWriteTime = SystemClock.Instance.GetCurrentInstant(), Length = 1011};
                        return file4;
                    }
                },
                root =>
                {
                    file1 = new EmptyFile(root, "file1") {LastWriteTime = SystemClock.Instance.GetCurrentInstant(), Length = 1234};
                    return file1;
                }                
            };

            var metaFileSystem = m_FileSystemToMetaFileSystemConverter.CreateMetaDirectory(expectedFileSystem);

            var convertedFileSystem = m_Instance.Convert(metaFileSystem);

            // check number of files and directories
            Assert.Equal(expectedFileSystem.Directories.Count(), convertedFileSystem.Directories.Count());
            Assert.Equal(expectedFileSystem.Files.Count(), convertedFileSystem.Files.Count());


            // compare files
            Assert.True(convertedFileSystem.FileExists(file1.Name));

            FileSystemAssert.FileEqual(file1, convertedFileSystem.GetFile(file1.Name));
            FileSystemAssert.FileEqual(file2, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file2.Name));
            FileSystemAssert.FileEqual(file3, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file3.Name));
            FileSystemAssert.FileEqual(file4, convertedFileSystem.GetDirectory(s_Dir2).GetFile(file4.Name));   
                               
        }

        [Fact]
        public void Convert_name_in_directory_properties_file_overrides_directory_name()
        {            
            var metaFileSystem = new Directory("root")
            {
                root => new Directory(root, System.IO.Path.GetRandomFileName())
                {
                    d => new DirectoryPropertiesFile(d, SystemClock.Instance.GetCurrentInstant(), new DirectoryProperties() { Name = s_Dir2})
                },
                root => new DirectoryPropertiesFile(root, SystemClock.Instance.GetCurrentInstant(), new DirectoryProperties() { Name = s_Dir1})
            };

            var convertedFileSystem = m_Instance.Convert(metaFileSystem);
            Assert.Empty(convertedFileSystem.Files);
            Assert.Equal(s_Dir1, convertedFileSystem.Name);
            Assert.Single(convertedFileSystem.Directories);
            Assert.Equal(s_Dir2, convertedFileSystem.Directories.Single().Name);
        }
    }
}