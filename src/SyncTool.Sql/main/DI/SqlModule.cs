using Autofac;
using SyncTool.Common;
using SyncTool.Common.Groups;
using SyncTool.Configuration;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using SyncTool.Sql.Services;
using SyncTool.Synchronization.State;

namespace SyncTool.Sql.DI
{
    public class SqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        { 
            //
            // Types required by group manager
            //
            builder.RegisterType<SqlGroupInitializer>().As<IGroupInitializer>();
            builder.RegisterType<SqlGroupValidator>().As<IGroupValidator>();
            
            //
            // Service implementations
            //
            // Configuration service
            builder.RegisterType<SqlConfigurationService>().As<IConfigurationService>().AsSelf();            
            // History service
            builder.RegisterType<SqlHistoryService>().As<IHistoryService>().AsSelf();
            builder.RegisterType<SqlFileSystemHistory>().AsSelf();
            builder.RegisterType<SqlFileSystemSnapshot>().AsSelf();
            builder.RegisterType<CachingSqlFileSystemFactory>().As<ISqlFileSystemFactory>().InstancePerMatchingLifetimeScope(Scope.Group);
            builder.RegisterType<SqlDirectory>().AsSelf();
            builder.RegisterType<SqlFile>().AsSelf();
            // multi-filesystem history service
            builder.RegisterType<SqlMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>();
            builder.RegisterType<SqlMultiFileSystemSnapshot>().AsSelf();
            // sync state service
            builder.RegisterType<SqlSyncStateService>().As<ISyncStateService>();
            builder.RegisterType<SqlSyncStateUpdater>().AsSelf();
            builder.RegisterType<SqlSyncAction>().AsSelf();
            builder.RegisterType<SqlSyncConflict>().AsSelf();

            //
            // Database access and repository implementations
            //
            builder.RegisterType<MySqlDatabase>().As<MySqlDatabase>();
            builder.RegisterType<MySqlDatabase>().As<Database>();
            builder.RegisterType<SyncFolderRepository>().AsSelf();
            builder.RegisterType<FileSystemHistoryRepository>().AsSelf();
            builder.RegisterType<SnapshotRepository>().AsSelf();
            builder.RegisterType<FileSystemRepository>().AsSelf();
            builder.RegisterType<MultiFileSystemSnapshotRepository>().AsSelf();
            builder.RegisterType<SyncStateRepository>().AsSelf();

            base.Load(builder);
        }        
    }
}