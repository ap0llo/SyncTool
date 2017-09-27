using System.IO;
using Microsoft.Extensions.Configuration;
using Autofac;
using SyncTool.Cli.Installation;
using SyncTool.Common.Options;
using SyncTool.Git.Options;

namespace SyncTool.Cli.Options
{
    sealed class OptionsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var root = GetConfiguration();

            builder.RegisterInstance(root).As<IConfiguration>();
            builder.RegisterOptions<UpdateOptions>(root);
            builder.RegisterOptions<GitOptions>(root);
            builder.RegisterOptions<ApplicationDataOptions>(root);
        }


        IConfiguration GetConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, ConfigFileNames.ApplicationConfigFile), optional: true)
                .AddJsonFile(Path.Combine(ApplicationInfo.RootDirectory, Path.ChangeExtension(ConfigFileNames.ApplicationConfigFile, ".Debug.json")), optional: true)
                .Build();

            return configuration;
        }
    }
}