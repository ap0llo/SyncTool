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


        public MySqlDatabase([NotNull] string server, uint port, [NotNull] string databaseName, [NotNull] string user, [NotNull] string password)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder()
            {
                Server = server ?? throw new ArgumentNullException(nameof(server)),
                Port = port,
                Database = databaseName ?? throw new ArgumentNullException(nameof(databaseName)),
                UserID = user ?? throw new ArgumentNullException(nameof(user)),
                Password = password ?? throw new ArgumentNullException(nameof(password))
            };
            
            ConnectionString = connectionStringBuilder.ConnectionString;
        }

        protected override IDbConnection DoOpenConnection()
        {            
            var connection = new MySqlConnection(ConnectionString);            
            connection.Open();

            return connection;
        }

        
    }
}