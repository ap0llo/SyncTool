using Autofac;
using SyncTool.Common.TestHelpers;
using SyncTool.Git.DI;
using SyncTool.Git.Options;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.TestHelpers
{
    public abstract class GitGroupBasedTest : GroupBasedTest
    {
        protected GitGroupBasedTest()
        {
            RepositoryInitHelper.InitializeRepository(m_RemotePath);            
        }

        protected override void RegisterServices(ContainerBuilder containerBuilder)
        {
            containerBuilder.RegisterModule<GitModuleFactoryModule>();
            containerBuilder.RegisterInstance(new GitOptions()).AsSelf();
        }
    }
}
