using System.IO;
using SyncTool.Git.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="SingleDirectoryRepositoryPathProvider"/>
    /// </summary>
    public class SingleDirectoryRepositoryPathProviderTest : DirectoryBasedTest
    {
        [Fact]
        public void Constructor_throws_DirectoryNotFoundException_if_home_directory_does_not_exist()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new SingleDirectoryRepositoryPathProvider(Path.Combine(m_TempDirectory.Location, "dir1")));
        }
    }
}