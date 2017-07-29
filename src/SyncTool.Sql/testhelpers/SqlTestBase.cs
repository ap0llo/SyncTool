using SyncTool.Sql.Model;
using System;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase
    {

        private class TestContextFactory : IDatabaseContextFactory
        {
            private readonly string m_DatabaseName;

            public TestContextFactory(string databaseName)
            {
                m_DatabaseName = databaseName;
            }

            public DatabaseContext CreateContext() => new InMemoryDatabaseContext(m_DatabaseName);
        }
        
        
        protected IDatabaseContextFactory ContextFactory { get; }


        public SqlTestBase()
        {                     
            ContextFactory = new TestContextFactory(Guid.NewGuid().ToString());
        }
        
    }
}
