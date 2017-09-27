using System;
using Autofac;
using JetBrains.Annotations;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Installation;
using SyncTool.Cli.Logging;
using SyncTool.Cli.Options;
using SyncTool.Common;
using SyncTool.Common.DI;
using SyncTool.FileSystem.DI;
using SyncTool.Git.DI;
using SyncTool.Sql.DI;
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
            using (var container = BuildContainer())
            {
                // initialize updater
                var updater = container.Resolve<Updater>();
                updater.Start();

                // run application
                int exitCode;
                using (var applicationScope = container.BeginLifetimeScope(Scope.Application))
                {
                    exitCode = applicationScope.Resolve<Application>().Run(args);
                }

                // wait for completion of updater
                if (updater.Status == UpdaterStatus.Running)
                {
                    Console.WriteLine("Application update is in progress, awaiting completion");
                }

                updater.Stop();

                if (updater.Status == UpdaterStatus.Failed)
                {
                    Console.WriteLine("Update failed: ");
                    Console.WriteLine("\t" + updater.Error.Replace("\n", "\n\t"));
                }

                return exitCode;
            }
        }

        static IContainer BuildContainer()
        {
            var containerBuilder = new ContainerBuilder();
            
            containerBuilder.RegisterModule<CliModule>();                    // add commandline application            
            containerBuilder.RegisterModule<OptionsModule>();                // load configuration            
            containerBuilder.RegisterModule<LoggingModule>();                // add logging
            containerBuilder.RegisterModule<UpdaterModule>();                // load updater
            containerBuilder.RegisterModule<GitModuleFactoryModule>();       // add support for git-based groups
            containerBuilder.RegisterModule<SqlModuleFactoryModule>();       // add support for database-backed groups
            containerBuilder.RegisterModule<CommonApplicationScopeModule>(); // SyncTool.Common
            containerBuilder.RegisterModule<FileSystemModule>();             // SyncTool.FileSystem
            containerBuilder.RegisterModule<SynchronizationModule>();        // SyncTool.Synchronization
            
            var container = containerBuilder.Build();
            return container;
        }
    }
}