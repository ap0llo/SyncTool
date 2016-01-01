// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Versioning;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitBasedGroupManager"/>
    /// </summary>
    public class GitBasedGroupManagerTest : DirectoryBasedTest
    {

        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".Groups is empty for empty home directory")]
        public void Groups_is_empty_for_empty_home_directory()
        {
            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".Groups: Non Git repositories are ignored")]
        public void Groups_Non_Git_repositories_are_ignored()
        {
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".Groups: Non Bare Repositories are ignored")]
        public void Groups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".GetGroup() throws " + nameof(GroupNotFoundException))]
        public void GetGroup_throws_SyncGroupNotFoundException()
        {
            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));            
            Assert.Throws<GroupNotFoundException>(() => groupManager.GetGroup("someName"));
        }

    }
}