using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlHistoryService : AbstractHistoryService
    {
        private readonly IDatabaseContextFactory m_ContextFactory;


        public override IEnumerable<IFileSystemHistory> Items
        {
            get
            {
                using (var context = m_ContextFactory.CreateContext())
                {
                    return context
                       .FileSystemHistories
                       .ToArray()
                       .Select(historyDo => new SqlFileSystemHistory(historyDo));
                }
            }
        }


        public SqlHistoryService(IDatabaseContextFactory contextFactory)
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }
        

        public override bool ItemExists(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
               return context.FileSystemHistories.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            }
        }


        protected override void DoCreateHistory(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var historyDo = new FileSystemHistoryDo() { Name = name };
                context.FileSystemHistories.Add(historyDo);
                context.SaveChanges();
            }
        }

        protected override IFileSystemHistory DoGetHistory(string name)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var historyDo = context.FileSystemHistories.Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
                return new SqlFileSystemHistory(historyDo);
            }
        }
    }
}
