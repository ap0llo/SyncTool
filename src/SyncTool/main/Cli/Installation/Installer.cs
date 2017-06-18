using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Squirrel;
using SyncTool.Cli.Configuration;

namespace SyncTool.Cli.Installation
{
    static class Installer
    {
        public static void HandleInstallationEvents()
        {
            var steps = new ExceptionLoggingInstallerStep(
                new CompositeInstallerStep(            
                    new ConfigFileInstallerStep(),
                    new InstallationFlagFileInstallerStep(),
                    new LauncherFileInstallerStep(),
                    new PathVariableInstallerStep()));

            SquirrelAwareApp.HandleEvents(
                onInitialInstall: steps.OnInitialInstall,
                onAppUpdate: steps.OnAppUpdate,
                onAppUninstall: steps.OnAppUninstall,
                onFirstRun: () =>
                {
                    Console.WriteLine($"{ApplicationInfo.ApplicationName} was installed");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Environment.Exit(0);
                });
        }
        
    }
}