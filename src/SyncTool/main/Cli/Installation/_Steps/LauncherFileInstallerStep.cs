using System;
using System.IO;
using System.Reflection;

namespace SyncTool.Cli.Installation
{
    public class LauncherFileInstallerStep : IInstallerStep
    {
        static string LauncherFilePath => Path.Combine(ApplicationInfo.RootDirectory, ApplicationInfo.ApplicationName.ToLowerInvariant() + ".bat");


        public void OnInitialInstall(Version version) => CreateLauncherFile();

        public void OnAppUpdate(Version version) => CreateLauncherFile();

        public void OnAppUninstall(Version version) => RemoveLauncherFile();

        
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

    }
}