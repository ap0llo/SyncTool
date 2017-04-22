using System;
using Xunit;

namespace SyncTool.FileSystem
{
    public class FileReferenceTest
    {
        [Fact(DisplayName = nameof(FileReference) + ": Constructor throws FormatException if path is empty or whitespace")]
        public void Constructor_throws_FormatException_if_path_is_empty_or_whitespace()
        {
            Assert.Throws<FormatException>(() => new FileReference(""));
            Assert.Throws<FormatException>(() => new FileReference("  "));
        }


        [Fact(DisplayName = nameof(FileReference) + ": Constructor throws FormatException if path contains a backslash")]
        public void Constructor_throws_FormatException_if_path_contains_a_backslash()
        {
            Assert.Throws<FormatException>(() => new FileReference("name\\name"));
        }

       
        [Fact(DisplayName = nameof(FileReference) + ": Constructor throws FormatException if path ends with directory separator char")]
        public void Constructor_throws_FormatException_if_path_ends_with_directory_separator_char()
        {
            Assert.Throws<FormatException>(() => new FileReference("name/"));
            Assert.Throws<FormatException>(() => new FileReference("name/someOtherName/"));
        }


        [Fact(DisplayName = nameof(FileReference) + ": Constructor throws FormatException if consists only of slashes")]
        public void Constructor_throws_FormatException_if_path_consits_only_of_slashes()
        {
            Assert.Throws<FormatException>(() => new FileReference("/"));
            Assert.Throws<FormatException>(() => new FileReference("//"));
            Assert.Throws<FormatException>(() => new FileReference("///"));
        }
    }
}