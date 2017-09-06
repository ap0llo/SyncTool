using Microsoft.Data.Sqlite;
using SyncTool.Sql.Model;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MySql.Data.MySqlClient;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase : IDisposable
    {
        class TestDatabase : Database
        {
            readonly string m_DatabaseName;
            readonly string m_SqlitePath;

            public override DatabaseLimits Limits { get; }

            public TestDatabase(string databaseName)
            {
                Limits = new DatabaseLimits(maxParameterCount: 999);
                m_DatabaseName = databaseName;
                m_SqlitePath = Path.Combine(Path.GetTempPath(), m_DatabaseName + ".db");
            }

            

            protected override IDbConnection DoOpenConnection()
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder()
                {
                    DataSource = m_SqlitePath, 
                    ["foreign keys"] = true
                };
                
                var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                return connection;
            }            
        }


        class MySqlTestDatabase : MySqlDatabase, IDisposable
        {
            readonly string m_DatabaseName;
            readonly string m_ConnectionString;

            public MySqlTestDatabase(string databaseName) : base("127.0.0.1", 3306, databaseName, "synctool", "synctool")
            {
                m_DatabaseName = databaseName;
                var connectionStringBuilder = new MySqlConnectionStringBuilder()
                {
                    Server = "127.0.0.1",
                    Port = 3306,
                    UserID = "synctool",
                    Password = "synctool",
                };
                m_ConnectionString = connectionStringBuilder.ConnectionString;

                using (var connection = new MySqlConnection(m_ConnectionString))
                {
                    connection.Open();
                    connection.ExecuteNonQuery($"CREATE DATABASE {m_DatabaseName} ;");
                }
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

        protected Database Database { get; }





        public SqlTestBase()
        {
            var databaseName = "synctool_test_" + Guid.NewGuid().ToString().Replace("-", "");            
            Database =new MySqlTestDatabase(databaseName);
        }

        static SqlTestBase()
        {
            LoadSqlite();
        }

        static void LoadSqlite()
        {
            var relativePath = IntPtr.Size == 8
                ? "runtimes\\win7-x64\\native"
                : "runtimes\\win7-x86\\native";

            var path = Path.Combine(Environment.CurrentDirectory, relativePath, "sqlite3.dll");

            _ = LoadLibraryEx(path, IntPtr.Zero, 0);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, int dwFlags);

        public void Dispose()
        {
            (Database as IDisposable)?.Dispose();
        }
    }
}
