using SyncTool.Sql.Model;
using System;

namespace SyncTool.Sql.TestHelpers
{
    public class SqlTestBase : IDisposable
    {
        protected readonly DatabaseContext m_Context;

        public SqlTestBase()
        {
            m_Context = new InMemoryDatabaseContext();
        }

        public void Dispose() => m_Context.Dispose();
    }
}
