using JetBrains.Annotations;
using SyncTool.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Configuration
{
    public abstract class AbstractConfigurationService : IConfigurationService
    {
        public abstract IEnumerable<SyncFolder> Items { get; }

        public SyncFolder this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                    throw new ArgumentNullException(nameof(name));

                var item = GetItemOrDefault(name);
                if (item == null)
                {
                    throw new ItemNotFoundException($"An item named '{name}' was not found");
                }
                return item;
            }
        }

        
        public void AddItem(SyncFolder folder)
        {
            if (folder == null)
            {
                throw new ArgumentNullException(nameof(folder));
            }

            if (ItemExists(folder.Name))
            {
                throw new DuplicateSyncFolderException(folder.Name);
            }

            DoAddItem(folder);
        }

        public void UpdateItem(SyncFolder folder)
        {
            if (folder == null)
                throw new ArgumentNullException(nameof(folder));

            if (!ItemExists(folder.Name))
                throw new SyncFolderNotFoundException($"A sync folder named '{folder.Name}' could not be found");

            DoUpdateItem(folder);
        }

        public virtual bool ItemExists(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            return Items.Any(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }


        protected virtual SyncFolder GetItemOrDefault(string name)
        {
            return Items.SingleOrDefault(f => f.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }


        protected abstract void DoAddItem([NotNull] SyncFolder folder);

        protected abstract void DoUpdateItem([NotNull] SyncFolder folder);
    }
}
