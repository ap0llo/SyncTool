using Ninject.Modules;

namespace SyncTool.Synchronization.DI
{
    public class SynchronizationModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ISynchronizer>().To<Synchronizer>();
        }
    }
}