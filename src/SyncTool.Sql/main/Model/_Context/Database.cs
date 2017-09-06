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
                    SchemaInfoTable.Create(connection, Limits);

                    FileSystemHistoriesTable.Create(connection, Limits);
                    FilesTable.Create(connection, Limits);
                    FileInstancesTable.Create(connection, Limits);
                    DirectoriesTable.Create(connection, Limits);
                    DirectoryInstancesTable.Create(connection, Limits);
                    ContainsDirectoryTable.Create(connection, Limits);
                    ContainsFileTable.Create(connection, Limits);
                    FileSystemSnapshotsTable.Create(connection, Limits);
                    IncludesFileInstanceTable.Create(connection, Limits);
                    SyncFoldersTable.Create(connection, Limits);

                    transaction.Commit();
                }
            }            
        }
    }
}
