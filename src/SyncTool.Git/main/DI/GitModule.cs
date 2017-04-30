using Autofac;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration;
using SyncTool.Git.Configuration.Reader;
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
            builder.RegisterType<JsonGroupSettingsProvider>().As<IGroupSettingsProvider>();

            builder.RegisterType<CurrentDirectoryRepositoryPathProvider>().As<IRepositoryPathProvider>();
            builder.RegisterType<GitBasedGroupManager>().As<IGroupManager>().InstancePerMatchingLifetimeScope(Scope.Application);

            //TODO: ExternallyOwned() is only a workaround
            builder.RegisterType<GitBasedGroup>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group).ExternallyOwned();

            builder.RegisterType<GitBasedConfigurationService>().As<IConfigurationService>();
            builder.RegisterType<GitBasedHistoryService>().As<IHistoryService>();
            builder.RegisterType<GitSyncPointService>().As<ISyncPointService>();
            builder.RegisterType<GitConflictService>().As<IConflictService>();
            builder.RegisterType<GitSyncActionService>().As<ISyncActionService>();
            builder.RegisterType<GitBasedMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>();
                       
            base.Load(builder);
        }        
    }
}