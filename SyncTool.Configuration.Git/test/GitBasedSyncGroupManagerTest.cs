// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.Configuration.Git
{
    public class GitBasedSyncGroupManagerTest : DirectoryBasedTest
    {

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ": Constructor throws DirectoryNotFoundException if home directory does not exist")]
        public void Constructor_throws_DirectoryNotFoundException_if_home_directory_does_not_exist()
        {
            Assert.Throws<DirectoryNotFoundException>(() => new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(Path.Combine(m_TempDirectory.Location, "dir1"))));
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".SyncGroups is empty for empty home directory")]
        public void SyncGroups_is_empty_for_empty_home_directory()
        {
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                Assert.Empty(groupManager.SyncGroups);
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".SyncGroups: Non Git repositories are ignored")]
        public void SyncGroups_Non_Git_repositories_are_ignored()
        {
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir1"));
            Directory.CreateDirectory(Path.Combine(m_TempDirectory.Location, "dir2"));

            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                Assert.Empty(groupManager.SyncGroups);
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".SyncGroups: Non Bare Repositories are ignored")]
        public void SyncGroups_Non_Bare_Repositories_are_ignored()
        {
            var dirPath = Path.Combine(m_TempDirectory.Location, "dir1");
            Directory.CreateDirectory(dirPath);

            Repository.Init((dirPath));

            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                Assert.Empty(groupManager.SyncGroups);
            }
        }


        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".GetSyncGroup() throws SyncGroupNotFoundException")]
        public void GetSyncGroup_throws_SyncGroupNotFoundException()
        {
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                groupManager.AddSyncGroup("group1");
                Assert.Throws<SyncGroupNotFoundException>(() => groupManager.GetSyncGroup("someName"));
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".GetSyncGroup() returns SyncGroup instance")]
        public void GetSyncGroup_returns_expected_SyncGroup()
        {
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                using (var expected = groupManager.AddSyncGroup("group1"))
                using (var actual = groupManager.GetSyncGroup("grOUp1"))
                {
                    Assert.NotNull(actual);
                    Assert.Equal(expected.Name, actual.Name);                    
                }                                
            }
        }



        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".CreateSyncGroup() creates local repository")]
        public void CreateSyncGroup_creates_local_repository()
        {
            var groupName = "group1";

            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                groupManager.AddSyncGroup(groupName);

                Assert.Single(groupManager.SyncGroups);
                Assert.Contains(groupName, groupManager.SyncGroups);
                Assert.True(Directory.Exists(Path.Combine(m_TempDirectory.Location, groupName)));
            }

        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".CreateSyncGroup() throws DuplicateSyncGroupException if a SyncGroup already exists")]
        public void CreateSyncGroup_throws_DuplicateSyncGroupException_if_a_SyncGroup_already_exists()
        {
            var groupName = "group1";
            
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                groupManager.AddSyncGroup(groupName);
                Assert.Throws<DuplicateSyncGroupException>(() => groupManager.AddSyncGroup(groupName.ToUpper()));
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".CreateSyncGroup() throws ConfigurationException if a directory already exists")]
        public void CreateSyncGroup_throws_ConfigurationException_if_a_directory_already_exists()
        {
            var groupName = "group1";

            var dirPath = Path.Combine(m_TempDirectory.Location, groupName);
            Directory.CreateDirectory(dirPath);

            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                Assert.Throws<ConfigurationException>(() => groupManager.AddSyncGroup(groupName));
            }
        }


        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".DeleteSyncGroup() removes SyncGroup")]
        public void DeleteSyncGroup_removes_SyncGroup()
        {
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                groupManager.AddSyncGroup("group1");
                groupManager.AddSyncGroup("group2");

                Assert.Equal(2, groupManager.SyncGroups.Count());

                groupManager.RemoveSyncGroup("group1");
                Assert.Single(groupManager.SyncGroups);
                Assert.False(Directory.Exists(Path.Combine(m_TempDirectory.Location, "group1")));

                groupManager.RemoveSyncGroup("group2");
                Assert.Empty(groupManager.SyncGroups);
                Assert.False(Directory.Exists(Path.Combine(m_TempDirectory.Location, "group2")));
            }

        }

        [Fact(DisplayName = nameof(GitBasedSyncGroupManager) + ".DeleteSyncGroup() throws SyncGroupNotFoundException")]
        public void DeleteSyncGroup_throws_SyncGroupNotFoundException()
        {
            using (var groupManager = new GitBasedSyncGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location)))
            {
                Assert.Throws<SyncGroupNotFoundException>(() => groupManager.RemoveSyncGroup("group1"));
            }
        }
    }
}