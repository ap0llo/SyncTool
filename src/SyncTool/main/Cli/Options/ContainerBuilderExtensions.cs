using System.IO;
using Microsoft.Extensions.Configuration;
using Autofac;
using SyncTool.Cli.Installation;
using SyncTool.Common.Options;

namespace SyncTool.Cli.Options
{
    static class ContainerBuilderExtensions
    {
        public const string ConfigFileName = "config.json";
        public const string DebugConfigFileName = "config.Debug.json";
        

        public static void RegisterConfiguration(this ContainerBuilder containerBuilder)
        {
            var root = GetConfigurationRoot();

            containerBuilder.RegisterOptions<UpdateOptions>(root);
            containerBuilder.RegisterOptions<UpdateOptions>(root);            
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