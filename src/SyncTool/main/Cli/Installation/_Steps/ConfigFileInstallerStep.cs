using System.IO;
using System.Reflection;
using System;
using SyncTool.Cli.Options;

namespace SyncTool.Cli.Installation
{
    class ConfigFileInstallerStep : IInstallerStep
    {
        const string s_DefaultConfigResourceName = "SyncTool.config.json";


        string ConfigFilePath => Path.Combine(ApplicationInfo.RootDirectory, ContainerBuilderExtensions.ConfigFileName);


        public void OnInitialInstall(Version version)
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(s_DefaultConfigResourceName))
            using (var reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                File.WriteAllText(ConfigFilePath, content);
            }
        }

        public void OnAppUpdate(Version version)
        {

        }

        public void OnAppUninstall(Version version)
        {
            File.Delete(ConfigFilePath);
        }
        
    }
}