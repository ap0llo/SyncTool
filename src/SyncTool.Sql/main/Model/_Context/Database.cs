using SyncTool.Sql.Model.Tables;
using System.Data;
using Dapper;

namespace SyncTool.Sql.Model
{
    public abstract class Database
    {
        public const int SchemaVersion = 1;

        static readonly object s_Lock = new object();
        bool m_Initialized = false;


        public abstract DatabaseLimits Limits { get; }


        public IDbConnection OpenConnection()
        {
            lock(s_Lock)
            {
                if(!m_Initialized)
                {
                    CheckSchema();
                    m_Initialized = true;
                }
            }

            return DoOpenConnection();
        }


        protected abstract IDbConnection DoOpenConnection();


        public void CheckSchema()
        {
            //TODO: Implement upgrade logic here when a new schema version is introduced            
            //TODO: Should schema upgrades be explicit or implicit?

            using (var connection = DoOpenConnection())
            {
                var schemaInfo = connection.QuerySingle<SchemaInfoDo>($"SELECT * FROM {SchemaInfoTable.Name}");

                if (schemaInfo.Version != SchemaVersion)
                {
                    throw new IncompatibleSchmeaException(
                        supportedVersion: SchemaVersion,
                        databaseVersion: schemaInfo.Version);
                }
            }
        }
        
        protected static void CreateSchema(IDbConnection connection, DatabaseLimits limits)
        {
            using (var transaction = connection.BeginTransaction())
            {
                // check if version table exists                
                FileSystemHistoriesTable.Create(connection, limits);
                FilesTable.Create(connection, limits);
                FileInstancesTable.Create(connection, limits);
                DirectoriesTable.Create(connection, limits);
                DirectoryInstancesTable.Create(connection, limits);
                ContainsDirectoryTable.Create(connection, limits);
                ContainsFileTable.Create(connection, limits);
                FileSystemSnapshotsTable.Create(connection, limits);
                IncludesFileInstanceTable.Create(connection, limits);
                SyncFoldersTable.Create(connection, limits);

                SchemaInfoTable.Create(connection, limits);

                transaction.Commit();                
            }
        }
    }
}
