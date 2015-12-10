// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.Configuration.Git
{
    public class GitBasedConfigurationGroupManagerTest : DirectoryBasedTest
    {

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".Groups is empty for empty home directory")]
        public void Groups_is_empty_for_empty_home_directory()
        {
            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".Groups: Non Git repositories are ignored")]
        public void Groups_Non_Git_repositories_are_ignored()
        {
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".Groups: Non Bare Repositories are ignored")]
        public void Groups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Empty(groupManager.Groups);
        }


        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".GetGroup() throws " + nameof(GroupNotFoundException))]
        public void GetGroup_throws_SyncGroupNotFoundException()
        {
            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            groupManager.AddGroup("group1");
            Assert.Throws<GroupNotFoundException>(() => groupManager.GetGroup("someName"));
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".GetGroup() returns SyncGroup instance")]
        public void AddGroup_returns_expected_SyncGroup()
        {
            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            groupManager.AddGroup("group1");

            using (var actual = groupManager.GetGroup("grOUp1"))
            {
                Assert.NotNull(actual);
                Assert.Equal("group1", actual.Name);
            }
        }



        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".AddGroup() creates local repository")]
        public void AddGroup_creates_local_repository()
        {
            var groupName = "group1";

            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            groupManager.AddGroup(groupName);

            Assert.Single(groupManager.Groups);
            Assert.Contains(groupName, groupManager.Groups);
            Assert.True(Directory.Exists(Path.Combine(m_TempDirectory.Location, groupName)));
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".AddGroup() throws DuplicateSyncGroupException if a SyncGroup already exists")]
        public void AddGroup_throws_DuplicateSyncGroupException_if_a_SyncGroup_already_exists()
        {
            var groupName = "group1";

            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            groupManager.AddGroup(groupName);
            Assert.Throws<DuplicateGroupException>(() => groupManager.AddGroup(groupName.ToUpper()));
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".AddGroup() throws ConfigurationException if a directory already exists")]
        public void AddGroup_throws_ConfigurationException_if_a_directory_already_exists()
        {
            var groupName = "group1";

            var dirPath = Path.Combine(m_TempDirectory.Location, groupName);
            Directory.CreateDirectory(dirPath);

            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));

            Assert.Throws<GroupManagerException>(() => groupManager.AddGroup(groupName));
        }


        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".RemoveGroup() removes SyncGroup")]
        public void RemoveGroup_removes_SyncGroup()
        {
            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));

            groupManager.AddGroup("group1");
            groupManager.AddGroup("group2");

            Assert.Equal(2, groupManager.Groups.Count());

            groupManager.RemoveGroup("group1");
            Assert.Single(groupManager.Groups);
            Assert.False(Directory.Exists(Path.Combine(m_TempDirectory.Location, "group1")));

            groupManager.RemoveGroup("group2");
            Assert.Empty(groupManager.Groups);
            Assert.False(Directory.Exists(Path.Combine(m_TempDirectory.Location, "group2")));
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationGroupManager) + ".RemoveGroup() throws " + nameof(GroupNotFoundException))]
        public void RemoveGroup_throws_SyncGroupNotFoundException()
        {
            var groupManager = new GitBasedConfigurationGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location));
            Assert.Throws<GroupNotFoundException>(() => groupManager.RemoveGroup("group1"));
        }
    }
}