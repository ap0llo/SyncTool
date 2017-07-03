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



        public static void RegisterConfiguration(this ContainerBuilder containerBuilder)
        {
            var root = GetConfigurationRoot();

            var updateConfiguration = root.GetSection(s_UpdateSectionName).Get<UpdateOptions>();
            updateConfiguration = updateConfiguration ?? new UpdateOptions();

            containerBuilder.RegisterInstance(updateConfiguration).AsSelf();
        }

        static IConfigurationRoot GetConfigurationRoot()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, ConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, DebugConfigFileName), optional: true)
                .Build();

            return configuration;
        }

       
    }
}