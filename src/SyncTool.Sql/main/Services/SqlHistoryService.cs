using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlHistoryService : AbstractHistoryService
    {
        private readonly DatabaseContext m_Context;


        public override IEnumerable<IFileSystemHistory> Items
        {
            get => m_Context
                    .FileSystemHistories
                    .ToArray()
                    .Select(historyDo => new SqlFileSystemHistory(historyDo));            
        }


        public SqlHistoryService(DatabaseContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
        }
        

        public override bool ItemExists(string name)
        {
            return m_Context.FileSystemHistories.Any(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }


        protected override void DoCreateHistory(string name)
        {
            var historyDo = new FileSystemHistoryDo() { Name = name };
            m_Context.FileSystemHistories.Add(historyDo);
            m_Context.SaveChanges();
        }

        protected override IFileSystemHistory DoGetHistory(string name)
        {
            var historyDo = m_Context.FileSystemHistories.Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return new SqlFileSystemHistory(historyDo);
        }
    }
}
