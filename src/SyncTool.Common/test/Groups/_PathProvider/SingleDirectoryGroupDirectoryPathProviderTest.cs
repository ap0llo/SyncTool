using System.IO;
using SyncTool.Common.TestHelpers;
using Xunit;
using SyncTool.Common.Groups;

namespace SyncTool.Common.Test.Groups
{
    /// <summary>
    /// Tests for <see cref="SingleDirectoryGroupDirectoryPathProvider"/>
    /// </summary>
    public class SingleDirectoryGroupDirectoryPathProviderTest
    {
        [Fact]
        public void Constructor_throws_DirectoryNotFoundException_if_root_directory_does_not_exist()
        {
            var nonExistingDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Assert.Throws<DirectoryNotFoundException>(() => new SingleDirectoryGroupDirectoryPathProvider(nonExistingDirectory));
        }
    }
}