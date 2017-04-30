using Autofac;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Git.Configuration.Reader;

namespace SyncTool.Git.DI
{
    public class GitModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<JsonGroupSettingsProvider>().As<IGroupSettingsProvider>();
            builder.RegisterType<CurrentDirectoryRepositoryPathProvider>().As<IRepositoryPathProvider>();
            builder.RegisterType<GitBasedGroupManager>().As<IGroupManager>();

            base.Load(builder);
        }        
    }
}