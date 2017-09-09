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
            var mysqlUri = Environment.GetEnvironmentVariable("SYNCTOOL_TEST_MYSQLURI");

            m_ConnectionString = new Uri(mysqlUri).ToMySqlConnectionString();
            using (var connection = new MySqlConnection(m_ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"CREATE DATABASE {m_DatabaseName} ;");
            }

            var connectionStringBuilder = new MySqlConnectionStringBuilder(m_ConnectionString)
            {
                Database = m_DatabaseName
            };            
            Database = new MySqlDatabase(connectionStringBuilder.ConnectionString);
        }

                       
        public void Dispose()
        {
            using (var connection = new MySqlConnection(m_ConnectionString))
            {
                connection.Open();
                connection.ExecuteNonQuery($"DROP DATABASE {m_DatabaseName} ;");
            }
        }
    }
}
