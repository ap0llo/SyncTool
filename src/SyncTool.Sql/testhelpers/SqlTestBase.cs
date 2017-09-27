using System;
using Microsoft.Extensions.Logging.Abstractions;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase : IDisposable
    {
        readonly Uri m_DatabaseUri;
        protected Database Database { get; }
        

        public SqlTestBase()
        {      
            // load database uri from environment variables      
            var mysqlUri = Environment.GetEnvironmentVariable("SYNCTOOL_TEST_MYSQLURI");
            
            // set database name 
            var uriBuilder = new UriBuilder(new Uri(mysqlUri))
            {
                Path = "synctool_test_" + Guid.NewGuid().ToString().Replace("-", "")
            };

            m_DatabaseUri = uriBuilder.Uri;

            // create database
            Database = new MySqlDatabase(NullLogger<MySqlDatabase>.Instance, m_DatabaseUri);
            Database.Create();            
        }
                
        
        public void Dispose() => Database.Drop();
    }
}
