using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.TestHelpers
{
    class InMemoryDatabaseContext : DatabaseContext
    {
        private readonly string m_DatabaseName;

        public InMemoryDatabaseContext(string databaseName)
        {
            m_DatabaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(m_DatabaseName);
        }
    }
}
