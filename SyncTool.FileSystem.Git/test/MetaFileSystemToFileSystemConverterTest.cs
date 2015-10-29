using System;
using System.IO;
using System.Linq;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class MetaFileSystemToFileSystemConverterTest
    {
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";

        readonly MetaFileSystemCreator m_MetaFileSystemCreator = new MetaFileSystemCreator();
        readonly MetaFileSystemToFileSystemConverter m_Instance = new MetaFileSystemToFileSystemConverter();


        [Fact]
        public void Convert()
        {
            var file1 = new EmptyFile("file1") {LastWriteTime = DateTime.Now, Length = 1234};
            var file2 = new EmptyFile("file2") {LastWriteTime = DateTime.Now, Length = 23456};
            var file3 = new EmptyFile("file3") {LastWriteTime = DateTime.Now, Length = 789};
            var file4 = new EmptyFile("file4") {LastWriteTime = DateTime.Now, Length = 1011};

            var expectedFileSystem = new Directory("root")
            {
                new Directory(s_Dir1)
                {
                    file2,
                    file3                    
                },
                new Directory(s_Dir2)
                {
                    file4
                },
                file1                
            };

            var metaFileSystem = m_MetaFileSystemCreator.CreateMetaDirectory(expectedFileSystem);

            var convertedFileSystem = m_Instance.Convert(metaFileSystem);

            // check number of files and directories
            Assert.Equal(expectedFileSystem.Directories.Count(), convertedFileSystem.Directories.Count());
            Assert.Equal(expectedFileSystem.Files.Count(), convertedFileSystem.Files.Count());


            // compare files
            Assert.True(convertedFileSystem.FileExists(file1.Name));

            AssertFileEquals(file1, convertedFileSystem.GetFile(file1.Name));
            AssertFileEquals(file2, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file2.Name));
            AssertFileEquals(file3, convertedFileSystem.GetDirectory(s_Dir1).GetFile(file3.Name));
            AssertFileEquals(file4, convertedFileSystem.GetDirectory(s_Dir2).GetFile(file4.Name));   
                               
        } 


        [Fact]
        public void Convert_name_in_directory_properties_file_overrides_directory_name()
        {            
            var metaFileSystem = new Directory("root")
            {
                new Directory(Path.GetRandomFileName())
                {
                    new DirectoryPropertiesFile(DateTime.Now, new DirectoryProperties() { Name = s_Dir2})
                },
                new DirectoryPropertiesFile(DateTime.Now, new DirectoryProperties() { Name = s_Dir1})
            };

            var convertedFileSystem = m_Instance.Convert(metaFileSystem);
            Assert.Empty(convertedFileSystem.Files);
            Assert.Equal(s_Dir1, convertedFileSystem.Name);
            Assert.Equal(1, convertedFileSystem.Directories.Count());
            Assert.Equal(s_Dir2, convertedFileSystem.Directories.Single().Name);
        }


        void AssertFileEquals(IFile expected, IFile actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            Assert.Equal(expected.LastWriteTime, actual.LastWriteTime);
        }

    }
}