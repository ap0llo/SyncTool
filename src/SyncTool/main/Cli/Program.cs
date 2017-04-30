using Autofac;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;

namespace SyncTool.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterModule<CliModule>();
            containerBuilder.RegisterModule<GitModule>();
            containerBuilder.RegisterModule<SynchronizationModule>();

            var container = containerBuilder.Build();

            using (var applicationScope = container.BeginLifetimeScope())
            {
                return container.Resolve<Application>().Run(args);
            }            
        }
    }
}