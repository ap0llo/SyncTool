using Autofac;
using SyncTool.Common.Groups;

namespace SyncTool.Sql.DI
{
    class SqlGroupModuleFactory : IGroupModuleFactory
    {
        public Module CreateModule() => new SqlModule();
    }
}
