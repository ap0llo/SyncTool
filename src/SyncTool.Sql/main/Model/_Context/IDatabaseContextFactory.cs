using System;
using System.Data;

namespace SyncTool.Sql.Model
{
    public interface IDatabaseContextFactory
    {
        [Obsolete]
        DatabaseContext CreateContext();

        IDbConnection OpenConnection();
    }
}
