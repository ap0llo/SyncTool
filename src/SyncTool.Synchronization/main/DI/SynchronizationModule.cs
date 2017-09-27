using Autofac;

namespace SyncTool.Synchronization.DI
{
    public class SynchronizationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Synchronizer>().As<ISynchronizer>();
            base.Load(builder);
        }        
    }
}