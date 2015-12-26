// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Tests for <see cref="SingleDirectoryRepositoryPathProvider"/>
    /// </summary>
    public class SingleDirectoryRepositoryPathProviderTest : DirectoryBasedTest
    {
        [Fact(DisplayName = nameof(SingleDirectoryRepositoryPathProvider) + ": Constructor throws DirectoryNotFoundException if home directory does not exist")]
        public void Constructor_throws_DirectoryNotFoundException_if_home_directory_does_not_exist()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new SingleDirectoryRepositoryPathProvider(Path.Combine(m_TempDirectory.Location, "dir1")));
        }
    }
}