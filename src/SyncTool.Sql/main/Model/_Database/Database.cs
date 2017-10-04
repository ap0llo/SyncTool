using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using JetBrains.Annotations;

namespace SyncTool.Sql.Model
{
    public abstract class Database
    {
        public const int SchemaVersion = 3;

        static readonly object s_Lock = new object();
        bool m_Initialized;
        readonly ILogger<Database> m_Logger;


        public abstract DatabaseLimits Limits { get; }


        protected Database([NotNull] ILogger<Database> logger)
        {
            m_Logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }


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

        public void Create()
        {
            DoCreateDatabase();
            DoCreateSchema();
        }

        public void CheckSchema()
        {
            //TODO: Implement upgrade logic here when a new schema version is introduced. Should schema upgrades be explicit or implicit?
            using (var connection = DoOpenConnection())
            {
                using (m_Logger.BeginScope("SchemaCheck"))
                {
                    m_Logger.LogInformation("Checking schema of database");
                    var schemaInfo = connection.QuerySingle<SchemaInfoDo>($"SELECT * FROM {SchemaInfoTable.Name}");

                    m_Logger.LogInformation($"Schema version of database is {schemaInfo.Version}, current version is {SchemaVersion}");

                    if (schemaInfo.Version != SchemaVersion)
                    {
                        throw new IncompatibleSchmeaException(
                            supportedVersion: SchemaVersion,
                            databaseVersion: schemaInfo.Version);
                    }
                }
            }
        }

        public abstract void Drop();


        protected abstract void DoCreateDatabase();

        protected abstract IDbConnection DoOpenConnection();


        void DoCreateSchema()
        {
            // create schema
            m_Logger.LogInformation($"Creating schema in database");

            using (var connection = DoOpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                // check if version table exists                
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
                MultiFileSystemSnapshotsTable.Create(connection, Limits);
                ContainsSnapshotTable.Create(connection, Limits);

                SchemaInfoTable.Create(connection, Limits);

                transaction.Commit();
            }
        }
    }
}
