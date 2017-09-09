using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace SyncTool.Sql.Model
{
    public class MySqlDatabase : Database
    {        
        public override DatabaseLimits Limits { get; } = new DatabaseLimits(65535);

        internal string ConnectionString { get; }


        public MySqlDatabase(Uri databaseUri)
        {
            ConnectionString = databaseUri.ToMySqlConnectionString();
        }
        

        protected override IDbConnection DoOpenConnection()
        {            
            var connection = new MySqlConnection(ConnectionString);            
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
                }

                // create a MySqlDatabase instance and open a connection
                // this will implicitly create the schema
                using(new MySqlDatabase(databaseUri).OpenConnection()) { }
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