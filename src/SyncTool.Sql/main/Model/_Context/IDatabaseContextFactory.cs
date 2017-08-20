using System.Data;

namespace SyncTool.Sql.Model
{
    public interface IDatabaseContextFactory
    {
        IDbConnection OpenConnection();
    }
}
