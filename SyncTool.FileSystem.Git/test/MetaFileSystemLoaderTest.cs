using System.IO;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class MetaFileSystemLoaderTest
    {
        const string s_File1 = "file1";


        readonly MetaFileSystemLoader m_Instance = new MetaFileSystemLoader();

        [Fact]
        public void Convert_replaces_files_with_FilePropertiesFile_instances()
        {            
            var input = new Directory(Path.GetRandomFileName())
            {
                FilePropertiesFile.ForFile(new EmptyFile(s_File1))
            };

            var directoryCreator = new CreateLocalDirectoryVisitor();
            using (var temporaryDirecoty = directoryCreator.CreateTemporaryDirectory(input))
            {
                var metaFs = m_Instance.Convert(temporaryDirecoty);
                
                Assert.Empty(metaFs.Directories);
                Assert.True(metaFs.GetFile(s_File1 + FilePropertiesFile.FileNameSuffix) is FilePropertiesFile);

            }
            
        } 
    }
}