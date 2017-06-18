using System;
using Autofac;
using JetBrains.Annotations;
using SyncTool.Cli.Configuration;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Installation;
using SyncTool.Common;
using SyncTool.Common.DI;
using SyncTool.FileSystem.FileSystem.DI;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;

namespace SyncTool.Cli
{
    public class Program
    {
        [UsedImplicitly]
        static int Main(string[] args)
        {           
            // handle installation events in case the application was launched by squirrel after installation
            Installer.HandleInstallationEvents();

            // load container
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterConfiguration();
            containerBuilder.RegisterType<Updater>().AsSelf().SingleInstance();

            containerBuilder.RegisterModule<CommonModule>();
            containerBuilder.RegisterModule<FileSystemModule>();
            containerBuilder.RegisterModule<GitModuleFactoryModule>();
            containerBuilder.RegisterModule<CliModule>();
            containerBuilder.RegisterModule<SynchronizationModule>();

            var container = containerBuilder.Build();

            // initialize updater
            var updater = container.Resolve<Updater>();

            // run application
            int exitCode;
            using (var applicationScope = container.BeginLifetimeScope(Scope.Application))
            {
                exitCode = applicationScope.Resolve<Application>().Run(args);
            }

            // wait for completion of updater
            if (updater.IsRunning)
            {
                Console.WriteLine("Application update is in progress, awaiting completion");
                updater.AwaitCompletion();                
            }

            return exitCode;
        }        
    }
}