// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Xunit;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Tests for <see cref="PathParser"/>
    /// </summary>
    public class PathParserTest
    {
        [Fact(DisplayName = nameof(PathParser) + ".GetFileName() returns expected result")]
        public void GetFileName_returns_expected_result()
        {
            Assert.Equal("file", PathParser.GetFileName("file"));
            Assert.Equal("file", PathParser.GetFileName("/file"));
            Assert.Equal("file", PathParser.GetFileName("/root/file"));
            Assert.Equal("file", PathParser.GetFileName("dir/file"));
        }

        [Fact(DisplayName = nameof(PathParser) + ".GetDirectoryName() returns expected result")]
        public void GetDirectoryName_returns_expected_result()
        {
            Assert.Equal("", PathParser.GetDirectoryName("file"));
            Assert.Equal("/", PathParser.GetDirectoryName("/file"));
            Assert.Equal("/root", PathParser.GetDirectoryName("/root/file"));
            Assert.Equal("dir", PathParser.GetDirectoryName("dir/file"));
        }

    }
}