using System.IO;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Common
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