using Autofac;
using SyncTool.Common.Groups;
using SyncTool.Git.Common.Groups;

namespace SyncTool.Git.DI
{
    public class GitModuleFactoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<GitGroupModuleFactory>().As<IGroupModuleFactory>();
        }
    }
}
