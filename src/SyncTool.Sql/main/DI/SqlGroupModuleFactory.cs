using System;
using Autofac;
using SyncTool.Common.Groups;

namespace SyncTool.Sql.DI
{
    class SqlGroupModuleFactory : IGroupModuleFactory
    {
        public bool IsAddressSupported(string address)
        {
            try
            {
                var uri = new Uri(address);
                return uri.IsSyncToolMySqlUri();
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public Module CreateModule() => new SqlModule();
    }
}
