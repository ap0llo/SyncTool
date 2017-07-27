using SyncTool.Configuration;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlConfigurationService : AbstractConfigurationService, IConfigurationService
    {
        readonly DatabaseContext m_Context;

                
        public override IEnumerable<SyncFolder> Items => m_Context.SyncFolders.Select(x => x.ToSyncFolder());


        public SqlConfigurationService(DatabaseContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public override bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return m_Context.SyncFolders.Any(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }

        protected override SyncFolder GetItemOrDefault(string name)
        {
            return m_Context.SyncFolders.SingleOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))?.ToSyncFolder();
        }

        protected override void DoAddItem(SyncFolder folder)
        {                        
            m_Context.Add(SyncFolderDo.FromSyncFolder(folder));
            m_Context.SaveChanges();
        }       

        protected override void DoUpdateItem(SyncFolder folder)
        {            
            var existingItem = m_Context.SyncFolders.Single(x => x.Name.Equals(folder.Name, StringComparison.InvariantCultureIgnoreCase));
            existingItem.Path = folder.Path;

            m_Context.Update(existingItem);            
        }
    }
}
