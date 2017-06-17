using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Squirrel;

namespace SyncTool.Cli.Installation
{
    static class Installer
    {

        static string InstallationRoot => Path.GetFullPath(Path.Combine(InstallationDirectory, "..")).TrimEnd(Path.DirectorySeparatorChar);

        static string InstallationDirectory => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).TrimEnd(Path.DirectorySeparatorChar);

        static string LauncherFilePath => Path.Combine(InstallationRoot, ApplicationName.ToLowerInvariant() + ".bat");

        static string ApplicationName => Assembly.GetExecutingAssembly().GetName().Name;


        public static void HandleInstallationEvents()
        {
            // Note, in most of these scenarios, the app exits after this method
            // completes!
            SquirrelAwareApp.HandleEvents(
                onInitialInstall: v =>
                {
                    CreateLauncherFile();
                    AddToPath();
                },
                onAppUpdate: v =>
                {
                    CreateLauncherFile();
                },
                onAppUninstall: v =>
                {
                    RemoveLauncherFile();
                    RemoveFromPath();
                },
                onFirstRun: () =>
                {
                    Console.WriteLine($"{ApplicationName} was installed and added to PATH.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Environment.Exit(0);
                });

        }

        
        static void CreateLauncherFile()
        {
            using (var fileStream = File.OpenWrite(LauncherFilePath))
            using (var writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("@ECHO OFF");
                writer.WriteLine($"\"{Assembly.GetExecutingAssembly().Location}\" %*");
                
            }
        }

        static void RemoveLauncherFile()
        {
            File.Delete(LauncherFilePath);                
        }

        static void AddToPath()
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);

            if (!currentValues.Contains(InstallationRoot))
            {
                Environment.SetEnvironmentVariable("PATH", value + ";" + InstallationRoot, EnvironmentVariableTarget.User);
            }            
        }

        static void RemoveFromPath()
        {
            var value = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) ?? "";
            var currentValues = new HashSet<string>(value.Split(';'), StringComparer.InvariantCultureIgnoreCase);


            var valuesToRemove = currentValues.Where(v => StringComparer.InvariantCultureIgnoreCase.Equals(v, InstallationDirectory));

            foreach (var path in valuesToRemove)
            {
                value = value.Replace(path, "");                
            }

            while (value.Contains(";;"))
            {
                value = value.Replace(";;", ";");
            }

            Environment.SetEnvironmentVariable("PATH", value + ";" + InstallationRoot, EnvironmentVariableTarget.User);
        }
        
    }
}