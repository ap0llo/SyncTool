using System;
using System.Data;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using SyncTool.Common.Groups;

namespace SyncTool.Sql.Model
{
    public class MySqlDatabase : Database
    {        
        static readonly DatabaseLimits s_MySqlDatabaseLimits = new DatabaseLimits(65535);

        readonly ILogger<MySqlDatabase> m_Logger;
        readonly Uri m_DatabaseUri;
        readonly string m_ConnectionString;


        public override DatabaseLimits Limits => s_MySqlDatabaseLimits;


        public MySqlDatabase(ILogger<MySqlDatabase> logger, GroupSettings groupSettings) 
            : this(logger, new Uri(groupSettings?.Address ?? throw new ArgumentNullException(nameof(groupSettings))))
        {
        }

        internal MySqlDatabase(ILogger<MySqlDatabase> logger, Uri databaseUri) : base(logger)
        {
            m_DatabaseUri = databaseUri ?? throw new ArgumentNullException(nameof(databaseUri));
            m_Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            m_ConnectionString = databaseUri.ToMySqlConnectionString();
        }


        public override void Drop()
        {
            var connectionStringBuilder = m_DatabaseUri.ToMySqlConnectionStringBuilder();

            if (String.IsNullOrEmpty(connectionStringBuilder.Database))
            {
                throw new DatabaseNameMissingException(m_DatabaseUri);
            }

            var databaseName = connectionStringBuilder.Database;
            connectionStringBuilder.Database = null;

            m_Logger.LogInformation($"Dropping database '{databaseName}'");
            using (var connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"DROP DATABASE {databaseName} ;");
            }
        }


        protected override IDbConnection DoOpenConnection()
        {            
            var connection = new MySqlConnection(m_ConnectionString);            
            connection.Open();

            return connection;
        }
        
        protected override void DoCreateDatabase()
        {
            var connectionStringBuilder = m_DatabaseUri.ToMySqlConnectionStringBuilder();

            if (String.IsNullOrEmpty(connectionStringBuilder.Database))
            {
                throw new DatabaseNameMissingException(m_DatabaseUri);
            }

            var databaseName = connectionStringBuilder.Database;
            connectionStringBuilder.Database = null;

            try
            {
                // create database 
                m_Logger.LogInformation($"Creating new database '{databaseName}'");
                using (var connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    connection.ExecuteNonQuery($"CREATE DATABASE {databaseName} ;");
                }
            }
            catch (MySqlException e)
            {
                throw new DatabaseException("Unhandled database error", e);
            }
        }
    }
}