using System.IO;
using System.Reflection;
using System;
using SyncTool.Cli.Options;

namespace SyncTool.Cli.Installation
{
    class ConfigFileInstallerStep : IInstallerStep
    {
        const string s_DefaultConfigResourceName = "SyncTool.config.json";
        const string s_NLogDefaultConfigResourceName = "SyncTool.nlog.config";


        string ConfigFilePath => Path.Combine(ApplicationInfo.RootDirectory, ConfigFileNames.ApplicationConfigFile);
        
        string NLogConfigFilePath => Path.Combine(ApplicationInfo.RootDirectory, ConfigFileNames.NLogConfigFile);


        public void OnInitialInstall(Version version)
        {
            SaveResource(s_DefaultConfigResourceName, ConfigFilePath, overwrite: true);
            SaveResource(s_NLogDefaultConfigResourceName, NLogConfigFilePath, overwrite: true);
        }

        public void OnAppUpdate(Version version)
        {
            SaveResource(s_DefaultConfigResourceName, ConfigFilePath, overwrite: false);
            SaveResource(s_NLogDefaultConfigResourceName, NLogConfigFilePath, overwrite: false);
        }

        public void OnAppUninstall(Version version)
        {
            File.Delete(ConfigFilePath);
            File.Delete(NLogConfigFilePath);
        }
        

        static void SaveResource(string resourceName, string path, bool overwrite)
        {
            if (File.Exists(path) && !overwrite)
                return;

            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                File.WriteAllText(path, content);
            }
        }
    }
}