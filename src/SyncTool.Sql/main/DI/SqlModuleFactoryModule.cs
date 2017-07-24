using Autofac;
using SyncTool.Common.Groups;
using SyncTool.Sql.Common.Groups;

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
