using System;
using Xunit;

namespace SyncTool.Synchronization.Conflicts
{
    public class ConflictInfoTest
    {


        [Fact]
        public void T01_Constructor_validates_file_path()
        {
            Assert.Throws<ArgumentNullException>(() => new ConflictInfo(null, null));
            Assert.Throws<FormatException>(() => new ConflictInfo("", null));
            Assert.Throws<FormatException>(() => new ConflictInfo(" ", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("/", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("fileName", null));
            Assert.Throws<FormatException>(() => new ConflictInfo("relative/path", null));
        }

    }
}