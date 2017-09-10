using Autofac;
using SyncTool.Common;
using SyncTool.Common.Groups;
using SyncTool.Configuration;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using SyncTool.Sql.Services;

namespace SyncTool.Sql.DI
{
    public class SqlModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        { 
            builder.RegisterType<SqlConfigurationService>().As<IConfigurationService>().AsSelf();
            builder.RegisterType<SyncFolderRepository>().AsSelf();
            builder.RegisterType<SqlGroupInitializer>().As<IGroupInitializer>();
            builder.RegisterType<SqlGroupValidator>().As<IGroupValidator>();

            builder.RegisterType<MySqlDatabase>().As<Database>();
            builder.RegisterType<SqlHistoryService>().As<IHistoryService>().AsSelf();
            builder.RegisterType<FileSystemHistoryRepository>().AsSelf();
            builder.RegisterType<SqlFileSystemHistory>().AsSelf();
            builder.RegisterType<SnapshotRepository>().AsSelf();
            builder.RegisterType<FileSystemRepository>().AsSelf();
            builder.RegisterType<SqlFileSystemSnapshot>().AsSelf();

            builder.RegisterType<CachingSqlFileSystemFactory>().As<ISqlFileSystemFactory>().InstancePerMatchingLifetimeScope(Scope.Group);
            builder.RegisterType<SqlDirectory>().AsSelf();
            builder.RegisterType<SqlFile>().AsSelf();

            /*                                             
            builder.RegisterType<GitSyncPointService>().As<ISyncPointService>().AsSelf();
            builder.RegisterType<GitConflictService>().As<IConflictService>().AsSelf();
            builder.RegisterType<GitSyncActionService>().As<ISyncActionService>().AsSelf();
            builder.RegisterType<GitBasedMultiFileSystemHistoryService>().As<IMultiFileSystemHistoryService>().AsSelf();
            builder.RegisterType<GitBasedFileSystemHistoryFactory>().AsSelf().InstancePerMatchingLifetimeScope(Scope.Group);
            builder.RegisterType<WorkingDirectoryFactory>().AsSelf();
            */

            base.Load(builder);
        }        
    }
}