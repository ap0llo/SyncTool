using Autofac;
using SyncTool.Common.Groups;

namespace SyncTool.Common.DI
{
    /// <summary>
    /// Module registering types in the Application scope
    /// </summary>
    public class CommonApplicationScopeModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Group>()
                .AsSelf()
                .As<IGroup>()                
                .ExternallyOwned();

            builder.RegisterType<CurrentDirectoryGroupDirectoryPathProvider>().As<IGroupDirectoryPathProvider>();
            builder.RegisterType<JsonGroupSettingsProvider>().As<IGroupSettingsProvider>();
            builder.RegisterType<GroupManager>().As<IGroupManager>().InstancePerMatchingLifetimeScope(Scope.Application);

            base.Load(builder);
        }
    }
}
