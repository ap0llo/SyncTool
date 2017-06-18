using System.IO;
using System.Reflection;

namespace SyncTool.Cli.Installation
{
    public static class ApplicationInfo
    {
        public static bool IsInstalled => InstallationFlagFileInstallerStep.IsInstalled;

        public static string RootDirectory
        {
            get
            {
                if (IsInstalled)
                {                    
                    return Path.GetFullPath(Path.Combine(CurrentVersionRootDirectory, "..")).TrimEnd(Path.DirectorySeparatorChar);
                }
                else
                {
                    return CurrentVersionRootDirectory;
                }
            }
        }

        public static string CurrentVersionRootDirectory
        {
            get
            {
                var assemblyLocation = Assembly.GetExecutingAssembly().Location;
                return Path.GetDirectoryName(assemblyLocation).TrimEnd(Path.DirectorySeparatorChar);
            }
        }

        public static string ApplicationName => Assembly.GetExecutingAssembly().GetName().Name;

    }
}