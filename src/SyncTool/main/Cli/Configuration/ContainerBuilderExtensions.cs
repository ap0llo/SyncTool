using System.IO;
using System.Reflection;
using Autofac;
using Microsoft.Extensions.Configuration;
using SyncTool.Cli.Installation;
using SyncTool.Git;

namespace SyncTool.Cli.Configuration
{
    static class ContainerBuilderExtensions
    {
        public const string ConfigFileName = "config.json";
        public const string DebugConfigFileName = "config.Debug.json";
        const string s_UpdateOptionsSectionName = "Update";
        const string s_GitOptionsSectionName = "Git";



        public static void RegisterConfiguration(this ContainerBuilder containerBuilder)
        {
            var root = GetConfigurationRoot();

            RegisterSection<UpdateOptions>(root, containerBuilder, s_UpdateOptionsSectionName);
            RegisterSection<GitOptions>(root, containerBuilder, s_GitOptionsSectionName);            
        }

        static IConfigurationRoot GetConfigurationRoot()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, ConfigFileName), optional: true)
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, DebugConfigFileName), optional: true)
                .Build();

            return configuration;
        }

        static void RegisterSection<T>(IConfigurationRoot configurationRoot, ContainerBuilder containerBuilder, string sectionName) where T : class, new()
        {
            var options = configurationRoot.GetSection(sectionName).Get<T>();
            options = options ?? new T();

            containerBuilder.RegisterInstance(options).AsSelf();
        }


    }
}