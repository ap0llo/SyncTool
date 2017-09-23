using System;
using Squirrel;

namespace SyncTool.Cli.Installation
{
    static class Installer
    {
        public static void HandleInstallationEvents()
        {
            var steps = new ExceptionLoggingInstallerStep(
                new CompositeInstallerStep(            
                    new InstallationFlagFileInstallerStep(), // this has to be the first step
                    new ConfigFileInstallerStep(),
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