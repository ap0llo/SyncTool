using SyncTool.Sql.Model.Tables;
using System.Data;

namespace SyncTool.Sql.Model
{
    public abstract class Database
    {
        static readonly object s_Lock = new object();
        bool m_Initialized = false;

        public abstract DatabaseLimits Limits { get; }

        public IDbConnection OpenConnection()
        {
            lock(s_Lock)
            {
                if(!m_Initialized)
                {
                    Initialize();
                    m_Initialized = true;
                }
            }

            return DoOpenConnection();
        }

        protected abstract IDbConnection DoOpenConnection();

        void Initialize()
        {            
            using (var connection = DoOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                // check if version table exists
                if(!connection.TableExists(SchemaInfoTable.Name))
                {
                    SchemaInfoTable.Create(connection);

                    FileSystemHistoriesTable.Create(connection);
                    FilesTable.Create(connection);
                    FileInstancesTable.Create(connection);
                    DirectoriesTable.Create(connection);
                    DirectoryInstancesTable.Create(connection);
                    ContainsDirectoryTable.Create(connection);
                    ContainsFileTable.Create(connection);
                    FileSystemSnapshotsTable.Create(connection);
                    IncludesFileInstanceTable.Create(connection);
                    SyncFoldersTable.Create(connection);

                    transaction.Commit();
                }
            }            
        }
    }
}
