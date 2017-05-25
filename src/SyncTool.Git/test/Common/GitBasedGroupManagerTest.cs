using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using Moq;
using SyncTool.Common;
using SyncTool.TestHelpers;
using Xunit;
using Autofac;
using SyncTool.FileSystem;

using static SyncTool.Git.TestHelpers.GroupSettingsProviderMockingHelper;
using Directory = System.IO.Directory;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitBasedGroupManager"/>
    /// </summary>
    public class GitBasedGroupManagerTest : DirectoryBasedTest
    {
    
        ILifetimeScope GetContainer(
            IGroupSettingsProvider settingsProvider = null,
            IGroupValidator groupValidator = null,
            IGroupInitializer groupInitializer = null)
        {            
            var builder = new ContainerBuilder();


            if(groupValidator != null)
                builder.RegisterInstance(groupValidator).As<IGroupValidator>();
            else
                builder.RegisterType<GitGroupValidator>().As<IGroupValidator>();
                

            if(groupInitializer != null)
                builder.RegisterInstance(groupInitializer).As<IGroupInitializer>();
            else
                builder.RegisterType<GitGroupInitializer>().As<IGroupInitializer>();


            builder
                .RegisterInstance(EqualityComparer<IFileReference>.Default)
                .As<IEqualityComparer<IFileReference>>();

            builder
                .RegisterInstance(new SingleDirectoryGroupDirectoryPathProvider(m_TempDirectory.Location))
                .As<IGroupDirectoryPathProvider>();

            builder.RegisterType<GitBasedGroupManager>().AsSelf();

            if (settingsProvider != null)
                builder.RegisterInstance(settingsProvider).As<IGroupSettingsProvider>();
            
            return builder.Build();
        }


        [Fact]
        public void GetGroup_throws_SyncGroupNotFoundException()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var instance = container.Resolve<GitBasedGroupManager>();
                Assert.Throws<GroupNotFoundException>(() => instance.GetGroup("someName"));
            }
        }


        //TODO: AddGroup

        #region AddGroup

        [Fact]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_name_already_exists()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithGroup("group1", "Irrelevant");
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var groupManager = container.Resolve<GitBasedGroupManager>();            
                Assert.Throws<DuplicateGroupException>(() => groupManager.AddGroup("Group1", "Irrelevant"));                
            }
        }

        [Fact]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithGroup("group1", "Address1");
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var instance = container.Resolve<GitBasedGroupManager>();
                Assert.Throws<DuplicateGroupException>(() => instance.AddGroup("Group2", "Address1"));
            }            
        }
    
        [Fact]        
        public void AddGroup_throws_GroupManagerException_if_validation_fails()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            var validatorMock = new Mock<IGroupValidator>(MockBehavior.Strict);
            validatorMock
                .Setup(m => m.EnsureGroupIsValid(It.IsAny<string>(), It.IsAny<string>()))
                .Throws<ValidationException>();

            using (var container = GetContainer(settingsProvider: settingsProviderMock.Object, groupValidator: validatorMock.Object))
            {
                var instance = container.Resolve<GitBasedGroupManager>();
                Assert.Throws<GroupManagerException>(() => instance.AddGroup("Group1", "Address1"));
            }            
        }
        
        [Fact]
        public void AddGroup_succeeds_for_a_repository_created_by_RepositoryInitHelper()
        {            
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            
            using (var container = GetContainer(settingsProviderMock.Object))
            using (var remote = CreateTemporaryDirectory())
            {
                var groupManager = container.Resolve<GitBasedGroupManager>();

                RepositoryInitHelper.InitializeRepository(remote.Location);
                groupManager.AddGroup("Group1", remote.Location);

                // AddGroup() should leave nothing behind in the local directory
                Assert.Empty(Directory.GetFileSystemEntries(m_TempDirectory.Location));
            }            
        }

        #endregion


        #region CreateGroup       

        [Fact]
        public void CreateGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_name_already_exists()
        {
            // create a mock for the settings provider
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            using (var container = GetContainer(settingsProviderMock.Object))
            using (var remote = CreateTemporaryDirectory())
            {
                // set up the "remote" repository
                Repository.Init(remote.Location, true);

                var groupManager = container.Resolve<GitBasedGroupManager>();

                // create a new group
                groupManager.CreateGroup("Group1", remote.Location);

                Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group1", "irrelevant"));
            }
        }

        [Fact]
        public void CreateGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            // create a mock for the settings provider
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            
            using (var container = GetContainer(settingsProviderMock.Object))
            using (var remote = CreateTemporaryDirectory())
            {
                // set up the "remote" repository
                Repository.Init(remote.Location, true);

                var groupManager = container.Resolve<GitBasedGroupManager>();

                // create a new group
                groupManager.CreateGroup("Group1", remote.Location);

                Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group2", remote.Location));
            }
        }

        #endregion

    }
}