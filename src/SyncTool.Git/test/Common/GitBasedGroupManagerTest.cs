// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Moq;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.Git.Configuration.Model;
using SyncTool.Git.Configuration.Reader;
using SyncTool.TestHelpers;
using Xunit;
using static SyncTool.Git.TestHelpers.GroupSettingsProviderMockingHelper;
using Directory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitBasedGroupManager"/>
    /// </summary>
    public class GitBasedGroupManagerTest : DirectoryBasedTest
    {


        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".GetGroup() throws " + nameof(GroupNotFoundException))]
        public void GetGroup_throws_SyncGroupNotFoundException()
        {
            Mock<IGroupSettingsProvider> settingsProvider = new Mock<IGroupSettingsProvider>(MockBehavior.Strict);
            settingsProvider.Setup(m => m.GetGroupSettings()).Returns(Enumerable.Empty<GroupSettings>());

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location), settingsProvider.Object);
            Assert.Throws<GroupNotFoundException>(() => groupManager.GetGroup("someName"));
        }


        //TODO: AddGroup


        #region AddGroup

        [Fact(DisplayName = nameof(GitBasedGroupManager) + "AddGroup() throws" + nameof(DuplicateGroupException) + " if a group with the specified name already exists")]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_name_already_exists()
        {
            var settingsProvider = GetGroupSettingsProviderMock().WithGroup("group1", "Irrelevant");
                      
            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location), settingsProvider.Object);
            
            Assert.Throws<DuplicateGroupException>(() => groupManager.AddGroup("Group1", "Irrelevant"));
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + "AddGroup() throws" + nameof(DuplicateGroupException) + " if a group with the specified address already exists")]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            var settingsProvider = GetGroupSettingsProviderMock().WithGroup("group1", "Address1");

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location), settingsProvider.Object);

            Assert.Throws<DuplicateGroupException>(() => groupManager.AddGroup("Group2", "Address1"));
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager) + "AddGroup() throws" + nameof(InvalidGroupAddressException) + " if the address does not point to a git repository")]
        public void AddGroup_throws_InvalidGroupAddressException_if_the_address_does_not_point_to_a_git_repository()
        {
            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(m_TempDirectory.Location), settingsProvider.Object);

            Assert.Throws<InvalidGroupAddressException>(() => groupManager.AddGroup("Group1", "Address1"));
        }


        [Fact(DisplayName = nameof(GitBasedGroupManager) + "AddGroup() throws" + nameof(InvalidGroupAddressException) + " if the address does not point to a group git repository")]
        public void AddGroup_throws_InvalidGroupAddressException_if_the_address_does_not_point_to_a_group_git_repository()
        {

            var localPath = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localPath);
            var remotePath = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(remotePath);

            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(Path.Combine(localPath)), settingsProvider.Object);
            
            
            Repository.Init(remotePath, true);

            Assert.Throws<InvalidGroupAddressException>(() => groupManager.AddGroup("Group1", remotePath));
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + "AddGroup() succeeds for a repository created by " + nameof(RepositoryInitHelper))]
        public void AddGroup_succeeds_for_a_repository_created_by_RepositoryInitHelper()
        {
            var localPath = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localPath);
            var remotePath = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(remotePath);

            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(localPath), settingsProvider.Object);

            RepositoryInitHelper.InitializeRepository(remotePath);

            groupManager.AddGroup("Group1", remotePath);

            // AddGroup() should leave nothing behind in the local directory
            Assert.Empty(Directory.GetFileSystemEntries(localPath));
        }

        #endregion


        #region CreateGroup

        [Fact(DisplayName = nameof(GitBasedGroupManager) + ".CreateGroup() creates a reporisoty and pushes it to the remote repository")]
        public void CreateGroup_Creates_a_repository_and_pushes_it_to_the_remote_repository()
        {
            // create a mock for the settings provider
            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            // set up local working directory
            var localDir = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localDir);

            // set up the "remote" repository
            var remoteDir = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(remoteDir);
            Repository.Init(remoteDir, true);

            
            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(localDir), settingsProvider.Object);

            // create a new group
            groupManager.CreateGroup("Group1", remoteDir);

            // creation of groups should not leave behind anything
            Assert.Empty(Directory.GetFileSystemEntries(localDir));

            // assert that the group was actually created in the remote repository
            using (var repository = new Repository(remoteDir))
            {
                Assert.Equal(2, repository.Branches.Count());
                Assert.True(repository.LocalBranchExists(RepositoryInitHelper.ConfigurationBranchName));
                Assert.NotNull(repository.Tags[RepositoryInitHelper.InitialCommitTagName]);
            }

            settingsProvider.Verify(m => m.SaveGroupSettings(It.IsAny<IEnumerable<GroupSettings>>()), Times.AtLeastOnce);
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + "CreateGroup() throws"+ nameof(DuplicateGroupException) + " if a group with the specified name already exists")]
        public void CreateGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_name_already_exists()
        {
            // create a mock for the settings provider
            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            // set up local working directory
            var localDir = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localDir);

            // set up the "remote" repository
            var remoteDir = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(remoteDir);
            Repository.Init(remoteDir, true);


            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(localDir), settingsProvider.Object);

            // create a new group
            groupManager.CreateGroup("Group1", remoteDir);

            Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group1", "irrelevant"));
        }

        [Fact(DisplayName = nameof(GitBasedGroupManager) + "CreateGroup() throws" + nameof(DuplicateGroupException) + " if a group with the specified address already exists")]
        public void CreateGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            // create a mock for the settings provider
            var settingsProvider = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            // set up local working directory
            var localDir = Path.Combine(m_TempDirectory.Location, "Local");
            Directory.CreateDirectory(localDir);

            // set up the "remote" repository
            var remoteDir = Path.Combine(m_TempDirectory.Location, "Remote");
            Directory.CreateDirectory(remoteDir);
            Repository.Init(remoteDir, true);


            var groupManager = new GitBasedGroupManager(new SingleDirectoryRepositoryPathProvider(localDir), settingsProvider.Object);

            // create a new group
            groupManager.CreateGroup("Group1", remoteDir);

            Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group2", remoteDir));
        }

        #endregion




    }
}