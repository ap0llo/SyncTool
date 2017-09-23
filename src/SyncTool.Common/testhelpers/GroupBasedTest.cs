using System;
using System.IO;
using Autofac;
using SyncTool.Common.DI;
using SyncTool.Common.Groups;
using SyncTool.Utilities;
using SyncTool.Common.Options;

using Directory = System.IO.Directory;

namespace SyncTool.Common.TestHelpers
{
    /// <summary>
    /// Test base class for tests that require a <see cref="Group"/>
    /// </summary>
    public abstract class GroupBasedTest : IDisposable
    {
        protected readonly string m_RemotePath;
        readonly TemporaryDirectory m_TempDirectory;        
        readonly IContainer m_Container;
        readonly ILifetimeScope m_ApplicationScope;
        

        protected GroupBasedTest()
        {
            m_TempDirectory = new TemporaryDirectory();

            m_RemotePath = Path.Combine(m_TempDirectory, "Remote");
            var groupSettingsDirectoy = Path.Combine(m_TempDirectory, "GroupSettings");
            var groupStorageRoot = Path.Combine(m_TempDirectory, "GroupStorage");

            Directory.CreateDirectory(m_RemotePath);
            Directory.CreateDirectory(groupStorageRoot);
            Directory.CreateDirectory(groupSettingsDirectoy);

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule<CommonApplicationScopeModule>();            
            
            containerBuilder.RegisterInstance(new JsonGroupSettingsProvider(groupSettingsDirectoy)).As<IGroupSettingsProvider>();
            containerBuilder
                .RegisterInstance(new GroupDirectoryPathProvider(new ApplicationDataOptions() { RootPath = groupStorageRoot }))
                .As<IGroupDirectoryPathProvider>();            


            RegisterServices(containerBuilder);

            m_Container = containerBuilder.Build();
            m_ApplicationScope = m_Container.BeginLifetimeScope(Scope.Application);
        }

        
        protected IGroup CreateGroup()
        {
            var groupManager = m_ApplicationScope.Resolve<IGroupManager>();

            groupManager.AddGroup("Group1", m_RemotePath);
                        
            return groupManager.OpenExclusively("Group1");
        }
        
        public virtual void Dispose()
        {
            m_ApplicationScope.Dispose();
            m_Container.Dispose();

            m_TempDirectory.Dispose();      
        }


        protected abstract void RegisterServices(ContainerBuilder containerBuilder);
    }
}