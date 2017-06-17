using System;
using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using Squirrel;
using SyncTool.Cli.Configuration;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Update;
using SyncTool.Common;
using SyncTool.Common.DI;
using SyncTool.FileSystem.FileSystem.DI;
using SyncTool.Git.DI;
using SyncTool.Synchronization.DI;

namespace SyncTool.Cli
{
    partial class Program
    {
        static int Main(string[] args)
        {                        
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterConfiguration();
            containerBuilder.RegisterType<Updater>().AsSelf().SingleInstance();

            containerBuilder.RegisterModule<CommonModule>();
            containerBuilder.RegisterModule<FileSystemModule>();
            containerBuilder.RegisterModule<GitModuleFactoryModule>();
            containerBuilder.RegisterModule<CliModule>();
            containerBuilder.RegisterModule<SynchronizationModule>();

            var container = containerBuilder.Build();

            var updater = container.Resolve<Updater>();

            int exitCode;
            using (var applicationScope = container.BeginLifetimeScope(Scope.Application))
            {
                exitCode = applicationScope.Resolve<Application>().Run(args);
            }

            if (updater.IsRunning)
            {
                Console.WriteLine("Application update is in progress, awaiting completion");
                updater.AwaitCompletion();                
            }

            return exitCode;
        }




    }
}