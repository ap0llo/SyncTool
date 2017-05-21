using Autofac;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Common;
using SyncTool.Common.DI;
using SyncTool.FileSystem.FileSystem.DI;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;

namespace SyncTool.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule<CommonModule>();
            containerBuilder.RegisterModule<FileSystemModule>();
            containerBuilder.RegisterModule<GitModule>();
            containerBuilder.RegisterModule<CliModule>();
            containerBuilder.RegisterModule<SynchronizationModule>();

            var container = containerBuilder.Build();

            using (var applicationScope = container.BeginLifetimeScope(Scope.Application))
            {
                return applicationScope.Resolve<Application>().Run(args);
            }            
        }
    }
}