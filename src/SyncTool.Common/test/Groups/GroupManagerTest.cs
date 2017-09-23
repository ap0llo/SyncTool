using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SyncTool.Common;
using SyncTool.Common.TestHelpers;
using Xunit;
using Autofac;
using System.IO;
using Autofac.Core.Lifetime;
using SyncTool.Utilities;
using SyncTool.Common.Groups;

using static SyncTool.Common.TestHelpers.GroupSettingsProviderMockingHelper;
using SyncTool.Common.Options;

namespace SyncTool.Common.Test.Groups
{
    /// <summary>
    /// Tests for <see cref="GroupManager"/>
    /// </summary>
    public class GroupManagerTest : IDisposable
    {

        #region Setup

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

        class DummyGroupValidator : IGroupValidator
        {
            public void EnsureGroupIsValid(string groupName, string address)
            { 
                // nop                
            }
        }

        class DummyGroupInitializer : IGroupInitializer
        {
            public void Initialize(string groupName, string address)
            {
                // nop
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
            moduleFactoryMock.Setup(m => m.IsAddressSupported(It.IsAny<string>())).Returns(true);

            builder
                .RegisterInstance(new GroupDirectoryPathProvider(new ApplicationDataOptions() { RootPath = m_TempDirectory }))
                .As<IGroupDirectoryPathProvider>();

            builder.RegisterType<GroupManager>().AsSelf();
            builder.RegisterType<Group>().As<IGroup>().AsSelf();


            if (settingsProvider != null)
                builder.RegisterInstance(settingsProvider).As<IGroupSettingsProvider>();

            builder.RegisterInstance(moduleFactoryMock.Object).As<IGroupModuleFactory>();

            return builder.Build();
        }


        ILifetimeScope GetContainerWithDummyDependencies()
        {
            return GetContainer(
                settingsProvider: GetGroupSettingsProviderMock().WithEmptyGroupSettings().Object,
                groupValidator: new DummyGroupValidator(),
                groupInitializer: new DummyGroupInitializer()
                );
        }

        #endregion
        
        #region OpenShared

        [Fact]
        public void OpenShared_throws_GroupNotFoundExceptin_if_Group_does_not_exist()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                Assert.Throws<GroupNotFoundException>(() => instance.OpenShared("Group"));
            }
        }

        [Fact]
        public void OpenShared_can_be_used_by_multiple_clients_simultaneously()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using (var opened1 = instance.OpenShared("Group1"))
                using (var opened2 = instance.OpenShared("Group1"))
                {                    
                }
            }
        }

        [Fact]
        public void OpenShared_throws_GroupOpenedException_if_group_is_opened_in_write_mode()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using (var group = instance.OpenExclusively("Group1"))
                {
                    Assert.Throws<GroupOpenedException>(() => instance.OpenShared("Group1"));
                }
            }
        }

        [Fact]
        public void OpenShared_ignores_case_in_group_names()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                instance.AddGroup("Group1", "Irrelevant");

                using (var group = instance.OpenShared("grOUp1"))
                {                    
                }
            }
        }

        [Fact]
        public void OpenShared_reuses_group_lifetime_scopes()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                instance.AddGroup("Group1", "Irrelevant");

                ILifetimeScope lifetime;
                using (var groupInstance1 = instance.OpenShared("Group1"))
                using (var groupInstance2 = instance.OpenShared("Group1"))
                {
                    Assert.Same(((Group)groupInstance1).LifetimeScope, ((Group)groupInstance2).LifetimeScope);                    
                    lifetime = ((Group)groupInstance1).LifetimeScope;
                }
            }
        }

        [Fact]
        public void OpenShared_reuses_group_lifetime_scopes_after_they_have_been_disposed()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                instance.AddGroup("Group1", "Irrelevant");

                ILifetimeScope lifetime1;
                ILifetimeScope lifetime2;

                using(var group = instance.OpenShared("Group1"))
                {
                    lifetime1 = ((Group) group).LifetimeScope;
                }

                using (var group = instance.OpenShared("Group1"))
                {
                    lifetime2 = ((Group)group).LifetimeScope;
                }

                Assert.Same(lifetime1, lifetime2);
            }
        }

        [Fact]
        public void OpenShared_creates_a_new_group_lifetime_scope_after_group_has_been_opened_exclusively()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                instance.AddGroup("Group1", "Irrelevant");

                var groupInstance1 = (Group) instance.OpenShared("Group1");
                groupInstance1.Dispose();                

                var groupInstance2 = (Group)instance.OpenExclusively("Group1");
                groupInstance2.Dispose();

                var groupInstance3 = (Group) instance.OpenShared("Group1");
                groupInstance3.Dispose();
                
                Assert.NotSame(groupInstance1.LifetimeScope, groupInstance2.LifetimeScope);
                Assert.NotSame(groupInstance1.LifetimeScope, groupInstance3.LifetimeScope);
                Assert.NotSame(groupInstance2.LifetimeScope, groupInstance3.LifetimeScope); 
            }
        }


        #endregion

        #region OpenExclusively

        [Fact]
        public void OpenExclusively_throws_GroupNotFoundExceptin_if_Group_does_not_exist()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                Assert.Throws<GroupNotFoundException>(() => instance.OpenExclusively("Group"));
            }
        }

        [Fact]
        public void OpenExclusively_can_be_used_again_after_group_was_disposed_01()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using(var group = instance.OpenExclusively("Group1"))
                {
                }

                using (var group = instance.OpenExclusively("Group1"))
                {
                }
            }
        }

        [Fact]
        public void OpenExclusively_can_be_used_again_after_group_was_disposed_02()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using (var group = instance.OpenShared("Group1"))
                {
                }

                using (var group = instance.OpenExclusively("Group1"))
                {
                }
            }
        }

        [Fact]
        public void OpenExclusively_throws_GroupOpenedException_if_group_if_already_opened_for_writing()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using (var group = instance.OpenExclusively("Group1"))
                {
                    Assert.Throws<GroupOpenedException>(() => instance.OpenExclusively("Group1"));
                }
            }
        }

        [Fact]
        public void OpenExclusively_throws_GroupOpenedException_if_group_is_opened_for_reading()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();

                instance.AddGroup("Group1", "SomeAddress");

                using (var group = instance.OpenShared("Group1"))
                {
                    Assert.Throws<GroupOpenedException>(() => instance.OpenExclusively("Group1"));
                }
            }
        }

        [Fact]
        public void OpenExclusively_ignores_case_in_group_names()
        {
            using (var container = GetContainerWithDummyDependencies())
            {
                var instance = container.Resolve<GroupManager>();
                instance.AddGroup("Group1", "Irrelevant");

                using (instance.OpenExclusively("grOUp1"))
                {
                    Assert.Throws<GroupOpenedException>(() => instance.OpenExclusively("Group1"));
                }
            }
        }

        #endregion

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
                .Throws<GroupValidationException>();

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
                .Throws<GroupInitializationException>();

            using (var container = GetContainer(settingsProviderMock.Object, null, initializerMock.Object))
            {
                var groupManager = container.Resolve<GroupManager>();                
                Assert.Throws<GroupManagerException>(() => groupManager.CreateGroup("Group1", "Address"));
            }
        }

        #endregion

    }
}