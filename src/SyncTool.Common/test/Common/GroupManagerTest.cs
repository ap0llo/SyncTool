using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SyncTool.Common;
using SyncTool.TestHelpers;
using Xunit;
using Autofac;
using System.IO;
using SyncTool.Utilities;

using static SyncTool.Common.TestHelpers.GroupSettingsProviderMockingHelper;

namespace SyncTool.Common.Test
{
    /// <summary>
    /// Tests for <see cref="GroupManager"/>
    /// </summary>
    public class GroupManagerTest : IDisposable
    {
        class TestGroupModule : Module
        {
            readonly IGroupInitializer m_GroupInitializer;
            readonly IGroupValidator m_GroupValidator;

            public TestGroupModule(IGroupValidator groupValidator, IGroupInitializer groupInitializer)
            {
                m_GroupValidator = groupValidator;
                m_GroupInitializer = groupInitializer;
            }

            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);

                builder.RegisterInstance(m_GroupValidator ?? Mock.Of<IGroupValidator>()).As<IGroupValidator>();
                builder.RegisterInstance(m_GroupInitializer ?? Mock.Of<IGroupInitializer>()).As<IGroupInitializer>();
            }
        }

        readonly string m_TempDirectory;


        public GroupManagerTest()
        {
            m_TempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(m_TempDirectory);
        }

        public void Dispose()
        {
            DirectoryHelper.DeleteRecursively(m_TempDirectory);
        }

        ILifetimeScope GetContainer(
            IGroupSettingsProvider settingsProvider = null,
            IGroupValidator groupValidator = null,
            IGroupInitializer groupInitializer = null)
        {            
            var builder = new ContainerBuilder();

            var groupModule = new TestGroupModule(groupValidator, groupInitializer);
            var moduleFactoryMock = new Mock<IGroupModuleFactory>();
            moduleFactoryMock.Setup(m => m.CreateModule()).Returns(groupModule);

            builder
                .RegisterInstance(new SingleDirectoryGroupDirectoryPathProvider(m_TempDirectory))
                .As<IGroupDirectoryPathProvider>();

            builder.RegisterType<GroupManager>().AsSelf();

            if (settingsProvider != null)
                builder.RegisterInstance(settingsProvider).As<IGroupSettingsProvider>();

            builder.RegisterInstance(moduleFactoryMock.Object).As<IGroupModuleFactory>();

            return builder.Build();
        }


        [Fact]
        public void GetGroup_throws_GroupNotFoundException()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var instance = container.Resolve<GroupManager>();
                Assert.Throws<GroupNotFoundException>(() => instance.GetGroup("someName"));
            }
        }


        //TODO: AddGroup

        #region AddGroup

        [Fact]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_name_already_exists()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithGroup("group1", "Some Address");
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var groupManager = container.Resolve<GroupManager>();            
                Assert.Throws<DuplicateGroupException>(() => groupManager.AddGroup("Group1", "Some other address"));                
            }
        }

        [Fact]
        public void AddGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            var settingsProviderMock = GetGroupSettingsProviderMock().WithGroup("group1", "Address1");
            using (var container = GetContainer(settingsProviderMock.Object))
            {
                var instance = container.Resolve<GroupManager>();
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
                var instance = container.Resolve<GroupManager>();
                Assert.Throws<GroupManagerException>(() => instance.AddGroup("Group1", "Address1"));
            }            
        }
        
        [Fact]
        public void AddGroup_succeeds_if_validation_succeeds()
        {            
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            var validatorMock = new Mock<IGroupValidator>();
            validatorMock.Setup(m => m.EnsureGroupIsValid(It.IsAny<string>(), It.IsAny<string>()));                

            using (var container = GetContainer(settingsProviderMock.Object))            
            {
                var groupManager = container.Resolve<GroupManager>();                
                groupManager.AddGroup("Group1", "Address");
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
            {
                var groupManager = container.Resolve<GroupManager>();

                // create a new group
                groupManager.CreateGroup("Group1", "Some Address");

                Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group1", "Some other address"));
            }
        }

        [Fact]
        public void CreateGroup_throws_DuplicateGroupException_if_a_group_with_the_specified_address_already_exists()
        {
            // create a mock for the settings provider
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();
            
            using (var container = GetContainer(settingsProviderMock.Object))            
            {                
                var groupManager = container.Resolve<GroupManager>();

                // create a new group
                groupManager.CreateGroup("Group1", "Address");

                Assert.Throws<DuplicateGroupException>(() => groupManager.CreateGroup("Group2", "Address"));
            }
        }

        [Fact]
        public void CreateGroup_throws_GroupMananagerException_when_initializer_fails()
        {
            // create a mock for the settings provider
            var settingsProviderMock = GetGroupSettingsProviderMock().WithEmptyGroupSettings();

            var initializerMock = new Mock<IGroupInitializer>();
            initializerMock
                .Setup(m => m.Initialize("Group1", "Address"))
                .Throws<InitializationException>();

            using (var container = GetContainer(settingsProviderMock.Object, null, initializerMock.Object))
            {
                var groupManager = container.Resolve<GroupManager>();                
                Assert.Throws<GroupManagerException>(() => groupManager.CreateGroup("Group1", "Address"));
            }
        }

        #endregion

    }
}