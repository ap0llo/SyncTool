using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    class DatabaseContextFactory : IDatabaseContextFactory
    {
        public DatabaseContext CreateContext() => new MySqlDatabaseContext();
    }
}
