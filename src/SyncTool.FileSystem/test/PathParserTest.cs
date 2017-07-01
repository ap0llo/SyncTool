using Xunit;

namespace SyncTool.FileSystem.Test
{
    /// <summary>
    /// Tests for <see cref="PathParser"/>
    /// </summary>
    public class PathParserTest
    {
        [Fact]
        public void GetFileName_returns_expected_result()
        {
            Assert.Equal("file", PathParser.GetFileName("file"));
            Assert.Equal("file", PathParser.GetFileName("/file"));
            Assert.Equal("file", PathParser.GetFileName("/root/file"));
            Assert.Equal("file", PathParser.GetFileName("dir/file"));
        }

        [Fact]
        public void GetDirectoryName_returns_expected_result()
        {
            Assert.Equal("", PathParser.GetDirectoryName("file"));
            Assert.Equal("/", PathParser.GetDirectoryName("/file"));
            Assert.Equal("/root", PathParser.GetDirectoryName("/root/file"));
            Assert.Equal("dir", PathParser.GetDirectoryName("dir/file"));
        }
    }
}