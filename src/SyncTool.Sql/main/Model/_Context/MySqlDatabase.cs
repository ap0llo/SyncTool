using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SyncTool.Sql.Model
{
    public class MySqlDatabase : Database
    {        
        static readonly DatabaseLimits s_MySqlDatabaseLimits = new DatabaseLimits(65535);
        readonly string m_ConnectionString;


        public override DatabaseLimits Limits => s_MySqlDatabaseLimits;

       
        public MySqlDatabase(Uri databaseUri)
        {
            m_ConnectionString = databaseUri.ToMySqlConnectionString();
        }
        

        protected override IDbConnection DoOpenConnection()
        {            
            var connection = new MySqlConnection(m_ConnectionString);            
            connection.Open();

            return connection;
        }

        
        public static void Create(Uri databaseUri)
        {
            var connectionStringBuilder = databaseUri.ToMySqlConnectionStringBuilder();

            if (String.IsNullOrEmpty(connectionStringBuilder.Database))
            {
                throw new DatabaseNameMissingException(databaseUri);
            }

            var databaseName = connectionStringBuilder.Database;
            connectionStringBuilder.Database = null;

            try
            {
                // create database
                using(var connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
                {
                    connection.Open();
                    connection.ExecuteNonQuery($"CREATE DATABASE {databaseName} ;");
                    CreateOrUpgradeSchema(connection, s_MySqlDatabaseLimits);
                }                
            }
            catch (MySqlException e)
            {                
                throw new DatabaseException("Unhandled database error", e);
            }
        }

        public static void Drop(Uri databaseUri)
        {
            var connectionStringBuilder = databaseUri.ToMySqlConnectionStringBuilder();

            if (String.IsNullOrEmpty(connectionStringBuilder.Database))
            {
                throw new DatabaseNameMissingException(databaseUri);
            }

            var databaseName = connectionStringBuilder.Database;
            connectionStringBuilder.Database = null;
            
            using (var connection = new MySqlConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"DROP DATABASE {databaseName} ;");
            }
        }
    }
}