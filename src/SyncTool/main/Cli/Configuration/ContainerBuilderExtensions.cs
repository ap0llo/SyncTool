using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;

namespace SyncTool.Cli.Configuration
{
    static class ContainerBuilderExtensions
    {
        const string s_ConfigFileName = "config.json";
        const string s_DebugConfigFileName = "config.Debug.json";
        const string s_UpdateSectionName = "Update";
        

        public static void RegisterConfiguration(this ContainerBuilder containerBuilder)
        {
            var root = GetConfigurationRoot();

            var updateConfiguration = root.GetSection(s_UpdateSectionName).Get<UpdateConfiguration>();
            updateConfiguration = updateConfiguration ?? new UpdateConfiguration();

            containerBuilder.RegisterInstance(updateConfiguration).AsSelf();
        }

        static IConfigurationRoot GetConfigurationRoot()
        {
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(directory, s_ConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(directory, s_DebugConfigFileName), optional: true)
                .Build();

            return configuration;
        }

    }
}