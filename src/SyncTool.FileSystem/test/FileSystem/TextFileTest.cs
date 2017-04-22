using System;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem
{
    public class TextFileTest : DirectoryBasedTest
    {
        [Fact]
        public void Null_content_is_disallowed()
        {
            Assert.Throws<ArgumentNullException>(() => new TextFile(null, "Irrelevant", null));
        }


        [Fact]
        public void Loading_from_file_returns_expected_value()
        {
            //ARRANGE
            var emptyTextFile = new TextFile(null, "file1.txt", "");       
            m_LocalItemCreator.CreateFile(emptyTextFile, m_TempDirectory.Location);

            //ACT            
            var readTextFile = TextFile.Load(null, (IReadableFile)  m_TempDirectory.Directory.GetFile(emptyTextFile.Name));
            Assert.Equal("", readTextFile.Content);
        }
    }
}