using System;
using System.IO;
using System.Reflection;

namespace SyncTool.Cli.Installation
{
    public class InstallationFlagFileInstallerStep : IInstallerStep
    {
        static string InstallationFlagFilePath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).TrimEnd(Path.DirectorySeparatorChar), "IsInstalled");

        public static bool IsInstalled => File.Exists(InstallationFlagFilePath);


        public void OnInitialInstall(Version version) => CreateInstallationFlagFile();

        public void OnAppUpdate(Version version) => CreateInstallationFlagFile();

        public void OnAppUninstall(Version version) => RemoveInstallationFlagFile();


        static void CreateInstallationFlagFile()
        {
            File.WriteAllText(InstallationFlagFilePath, "");
        }

        static void RemoveInstallationFlagFile()
        {
            File.Delete(InstallationFlagFilePath);
        }

    }
}