// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem.Git;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git
{
    /// <summary>
    /// Tests for <see cref="GitBasedHistoryGroupManager"/>
    /// </summary>
    public class GitBasedHistoryGroupManagerTest : DirectoryBasedTest
    {

        [Fact(DisplayName = nameof(GitBasedHistoryGroupManager) + ".HistoryRepositories is empty for empty home directory")]
        public void SyncGroups_is_empty_for_empty_home_directory()
        {
            var groupManager = new GitBasedHistoryGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedHistoryGroupManager) + ".HistoryRepositories: Non Git repositories are ignored")]
        public void SyncGroups_Non_Git_repositories_are_ignored()
        {
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            var groupManager = new GitBasedHistoryGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedHistoryGroupManager) + ".HistoryRepositories: Non Bare Repositories are ignored")]
        public void SyncGroups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            System.IO.Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            var groupManager = new GitBasedHistoryGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedHistoryGroupManager) + ".GetHistoryRepository() throws " + nameof(HistoryGroupNotFoundException))]
        public void GetSyncGroup_throws_SyncGroupNotFoundException()
        {
            var groupManager = new GitBasedHistoryGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));            
            Assert.Throws<HistoryGroupNotFoundException>(() => groupManager.GetGroup("someName"));
        }

    }
}