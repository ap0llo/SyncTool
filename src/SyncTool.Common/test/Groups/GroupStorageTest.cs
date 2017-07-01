using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SyncTool.Common.Groups;

namespace SyncTool.Common.Test.Groups
{
    public class GroupStorageTest
    {

        [Fact]
        public void Constructor_throws_ArgumentException_if_path_is_null_or_whitespace()
        {
            Assert.Throws<ArgumentException>(() => new GroupStorage(null));
            Assert.Throws<ArgumentException>(() => new GroupStorage(""));
            Assert.Throws<ArgumentException>(() => new GroupStorage(" "));
            Assert.Throws<ArgumentException>(() => new GroupStorage("\t"));
        }


        [Fact]
        public void Constructor_throws_DirectoryNotFoundException_if_path_does_not_exist()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<DirectoryNotFoundException>(() => new GroupStorage(randomPath));
        }

        [Fact]
        public void Constructor_succeeds_for_existing_path()
        {
            var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(randomPath);

            _ = new GroupStorage(randomPath);

            Directory.Delete(randomPath);
        }


        [Fact]
        public void Constructor_converts_path_to_full_Path()
        {
            var name = Path.GetRandomFileName();
            var fullPath = Path.Combine(Path.GetTempPath(), name);
            Directory.CreateDirectory(fullPath);
            var path = Path.Combine(Path.GetTempPath(), name, "..", name);

            var instance = new GroupStorage(path);
            Assert.Equal(fullPath, instance.Path);

            Directory.Delete(fullPath);
        }

    }
}
