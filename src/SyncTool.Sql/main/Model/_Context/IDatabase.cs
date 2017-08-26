using System.Data;

namespace SyncTool.Sql.Model
{
    public interface IDatabase
    {
        IDbConnection OpenConnection();
    }
}
