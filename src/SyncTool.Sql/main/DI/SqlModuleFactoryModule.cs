using Autofac;
using SyncTool.Common.Groups;

namespace SyncTool.Sql.DI
{
    public class SqlModuleFactoryModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<SqlGroupModuleFactory>().As<IGroupModuleFactory>();
        }
    }
}
