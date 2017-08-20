﻿using Microsoft.Data.Sqlite;
using SyncTool.Sql.Model;
using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase
    {
        class TestContextFactory : IDatabaseContextFactory
        {
            readonly string m_DatabaseName;
            readonly string m_SqlitePath;

            public TestContextFactory(string databaseName)
            {
                m_DatabaseName = databaseName;
                m_SqlitePath = Path.Combine(Path.GetTempPath(), m_DatabaseName + ".db");
            }

          
            public IDbConnection OpenConnection()
            {
                var connectionStringBuilder = new SqliteConnectionStringBuilder()
                {
                    DataSource = m_SqlitePath
                };
                var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);
                connection.Open();
                return connection;
            }
        }
        
        
        protected IDatabaseContextFactory ContextFactory { get; }


        public SqlTestBase()
        {                     
            ContextFactory = new TestContextFactory(Guid.NewGuid().ToString());
            
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
    }
}
