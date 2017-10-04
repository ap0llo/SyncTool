using Autofac;
using SyncTool.Common;

namespace SyncTool.Synchronization.DI
{
    public class SynchronizationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Synchronizer>().As<ISynchronizer>().InstancePerMatchingLifetimeScope(Scope.Group);
            base.Load(builder);
        }        
    }
}