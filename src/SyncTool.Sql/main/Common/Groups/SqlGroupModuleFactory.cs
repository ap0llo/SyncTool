using Autofac;
using SyncTool.Common.Groups;
using SyncTool.Sql.DI;

namespace SyncTool.Sql.Common.Groups
{
    class SqlGroupModuleFactory : IGroupModuleFactory
    {
        public Module CreateModule() => new SqlModule();
    }
}
