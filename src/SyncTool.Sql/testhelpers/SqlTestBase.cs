using SyncTool.Sql.Model;
using System;
using MySql.Data.MySqlClient;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase : IDisposable
    {
        readonly string m_DatabaseName = "synctool_test_" + Guid.NewGuid().ToString().Replace("-", "");
        readonly string m_ConnectionString;

        protected Database Database { get; }
        

        public SqlTestBase()
        {      
            // load database uri from environment variables      
            var mysqlUri = Environment.GetEnvironmentVariable("SYNCTOOL_TEST_MYSQLURI");

            // get connection string without database name
            var connectionStringBuilder = new Uri(mysqlUri).ToMySqlConnectionStringBuilder();
            connectionStringBuilder.Database = null;
            m_ConnectionString = connectionStringBuilder.ConnectionString;            

            // create database to run tests against
            using (var connection = new MySqlConnection(m_ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"CREATE DATABASE {m_DatabaseName} ;");
            }

            // create Database instance that will be used by tests
            // this time inlcude the database name in the connection string
            connectionStringBuilder.Database = m_DatabaseName;
            Database = new MySqlDatabase(connectionStringBuilder.ConnectionString);
        }

                       
        public void Dispose()
        {
            // delete test database
            using (var connection = new MySqlConnection(m_ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"DROP DATABASE {m_DatabaseName} ;");
            }
        }
    }
}
