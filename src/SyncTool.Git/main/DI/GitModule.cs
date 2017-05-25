using Autofac;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.Synchronization.Conflicts;
using SyncTool.Git.Synchronization.State;
using SyncTool.Git.Synchronization.SyncActions;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Git.DI
{
    public class GitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            
            
            builder.RegisterType<GitBasedGroupManager>().As<IGroupManager>().InstancePerMatchingLifetimeScope(Scope.Application);
                       

            builder.RegisterType<GitRepository>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);

            builder.RegisterType<GitBasedConfigurationService>().As<IConfigurationService>().AsSelf();
            builder.RegisterType<GitBasedHistoryService>().As<IHistoryService>().AsSelf();
            builder.RegisterType<GitSyncPointService>().As<ISyncPointService>().AsSelf();
            builder.RegisterType<GitConflictService>().As<IConflictService>().AsSelf();
            builder.RegisterType<GitSyncActionService>().As<ISyncActionService>().AsSelf();
            builder.RegisterType<GitBasedMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>().AsSelf();
            builder.RegisterType<GitGroupValidator>().As<IGroupValidator>();
            builder.RegisterType<GitGroupInitializer>().As<IGroupInitializer>();

            base.Load(builder);
        }        
    }
}