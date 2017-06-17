using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using SyncTool.Cli.Installation;

namespace SyncTool.Cli.Configuration
{
    static class ContainerBuilderExtensions
    {
        public const string ConfigFileName = "config.json";
        public const string DebugConfigFileName = "config.Debug.json";
        const string s_UpdateSectionName = "Update";


        static string ConfigurationDirectory
        {
            get
            {
                var dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (Installer.IsInstalled)
                {
                    dir = Path.Combine(dir, "..");
                }
                return Path.GetFullPath(dir);
            }
        }

        public static void RegisterConfiguration(this ContainerBuilder containerBuilder)
        {
            var root = GetConfigurationRoot();

            var updateConfiguration = root.GetSection(s_UpdateSectionName).Get<UpdateConfiguration>();
            updateConfiguration = updateConfiguration ?? new UpdateConfiguration();

            containerBuilder.RegisterInstance(updateConfiguration).AsSelf();
        }

        static IConfigurationRoot GetConfigurationRoot()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ConfigurationDirectory, ConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(ConfigurationDirectory, DebugConfigFileName), optional: true)
                .Build();

            return configuration;
        }

       
    }
}