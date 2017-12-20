using SyncTool.Utilities;
using System;
using Xunit;
using SyncTool.Common.Groups;
using Grynwald.Utilities.IO;

namespace SyncTool.Common.Test.Groups
{
    public class GroupSettingsTest
    {

        [Fact]
        public void Name_must_not_be_null_or_whitespace()
        {
            using (var directory = new TemporaryDirectory())
            {
                Assert.Throws<ArgumentNullException>(() => new GroupSettings(null, directory));
                Assert.Throws<ArgumentException>(() => new GroupSettings("", directory));
                Assert.Throws<ArgumentException>(() => new GroupSettings("  ", directory));
                Assert.Throws<ArgumentException>(() => new GroupSettings(" \t ", directory));
            }
        }


        [Fact]
        public void Address_must_not_be_null_or_whitespace()
        {
            Assert.Throws<ArgumentNullException>(() => new GroupSettings("GroupName", null));
            Assert.Throws<ArgumentException>(() => new GroupSettings("GroupName", ""));
            Assert.Throws<ArgumentException>(() => new GroupSettings("GroupName", " "));
            Assert.Throws<ArgumentException>(() => new GroupSettings("GroupName", "\t"));            
        }


    }
}
