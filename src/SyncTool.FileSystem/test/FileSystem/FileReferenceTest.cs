using System;
using Xunit;

namespace SyncTool.FileSystem
{
    public class FileReferenceTest
    {
        [Fact]
        public void Constructor_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => new FileReference(""));
            Assert.Throws<FormatException>(() => new FileReference("  "));
        }


        [Fact]
        public void Constructor_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => new FileReference("name\\name"));
        }


        [Fact]
        public void Constructor_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => new FileReference("name/"));
            Assert.Throws<FormatException>(() => new FileReference("name/someOtherName/"));
        }


        [Fact]
        public void Constructor_throws_FormatException_if_path_consits_only_of_slashes()
        {
            Assert.Throws<FormatException>(() => new FileReference("/"));
            Assert.Throws<FormatException>(() => new FileReference("//"));
            Assert.Throws<FormatException>(() => new FileReference("///"));
        }
    }
}