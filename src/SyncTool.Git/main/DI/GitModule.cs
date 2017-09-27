using Autofac;
using SyncTool.Common;
using SyncTool.Common.Groups;
using SyncTool.Configuration;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common.Groups;
using SyncTool.Git.Configuration;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.DI
{
    public class GitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {                                              
            builder.RegisterType<GitRepository>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);

            builder.RegisterType<GitBasedConfigurationService>().As<IConfigurationService>().AsSelf();
            builder.RegisterType<GitBasedHistoryService>().As<IHistoryService>().AsSelf();
            builder.RegisterType<GitBasedMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>().AsSelf();
            builder.RegisterType<GitGroupValidator>().As<IGroupValidator>();
            builder.RegisterType<GitGroupInitializer>().As<IGroupInitializer>();
            builder.RegisterType<GitBasedFileSystemHistoryFactory>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);
            builder.RegisterType<WorkingDirectoryFactory>().AsSelf();

            base.Load(builder);
        }        
    }
}