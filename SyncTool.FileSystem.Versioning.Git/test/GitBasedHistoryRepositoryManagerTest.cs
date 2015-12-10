// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitBasedHistoryRepositoryManagerTest : DirectoryBasedTest
    {




        [Fact(DisplayName = nameof(GitBasedHistoryRepositoryManager) + ".HistoryRepositories is empty for empty home directory")]
        public void SyncGroups_is_empty_for_empty_home_directory()
        {
            var groupManager = new GitBasedHistoryRepositoryManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.HistoryRepositories);
        }

        [Fact(DisplayName = nameof(GitBasedHistoryRepositoryManager) + ".HistoryRepositories: Non Git repositories are ignored")]
        public void SyncGroups_Non_Git_repositories_are_ignored()
        {
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            System.IO.Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            var groupManager = new GitBasedHistoryRepositoryManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.HistoryRepositories);
        }

        [Fact(DisplayName = nameof(GitBasedHistoryRepositoryManager) + ".HistoryRepositories: Non Bare Repositories are ignored")]
        public void SyncGroups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            System.IO.Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            var groupManager = new GitBasedHistoryRepositoryManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.HistoryRepositories);
        }


        [Fact(DisplayName = nameof(GitBasedHistoryRepositoryManager) + ".GetHistoryRepository() throws " + nameof(HistoryRepositoryNotFoundException))]
        public void GetSyncGroup_throws_SyncGroupNotFoundException()
        {
            var groupManager = new GitBasedHistoryRepositoryManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));            
            Assert.Throws<HistoryRepositoryNotFoundException>(() => groupManager.GetHistoryRepository("someName"));
        }

     




    }
}