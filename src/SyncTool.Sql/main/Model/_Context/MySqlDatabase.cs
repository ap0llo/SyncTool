using System;
using System.Data;
using JetBrains.Annotations;
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
    }
}