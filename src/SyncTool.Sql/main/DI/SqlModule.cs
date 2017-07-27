using Autofac;
using SyncTool.Configuration;
using SyncTool.Sql.Services;

namespace SyncTool.Sql.DI
{
    public class SqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        { 
            builder.RegisterType<SqlConfigurationService>().As<IConfigurationService>().AsSelf();

            /*                                             
            builder.RegisterType<GitRepository>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);

            builder.RegisterType<GitBasedHistoryService>().As<IHistoryService>().AsSelf();
            builder.RegisterType<GitSyncPointService>().As<ISyncPointService>().AsSelf();
            builder.RegisterType<GitConflictService>().As<IConflictService>().AsSelf();
            builder.RegisterType<GitSyncActionService>().As<ISyncActionService>().AsSelf();
            builder.RegisterType<GitBasedMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>().AsSelf();
            builder.RegisterType<GitGroupValidator>().As<IGroupValidator>();
            builder.RegisterType<GitGroupInitializer>().As<IGroupInitializer>();
            builder.RegisterType<GitBasedFileSystemHistoryFactory>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);
            builder.RegisterType<WorkingDirectoryFactory>().AsSelf();

            */            
            base.Load(builder);
        }        
    }
}