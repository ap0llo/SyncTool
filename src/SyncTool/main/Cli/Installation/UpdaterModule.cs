using Autofac;

namespace SyncTool.Cli.Installation
{
    sealed class UpdaterModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Updater>().AsSelf().SingleInstance();
            base.Load(builder);
        }
    }
}
