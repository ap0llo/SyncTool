using SyncTool.Common.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.FileSystem.Versioning
{
    public abstract class AbstractHistoryService : IHistoryService
    {
        public IFileSystemHistory this[string name]
        {
            get
            {
                if (String.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(name));
                }

                if (!ItemExists(name))
                {
                    throw new ItemNotFoundException($"An filesystem history named '{name}' was not found");
                }

                return DoGetHistory(name);
            }
        }

        public abstract IEnumerable<IFileSystemHistory> Items { get; }


        public void CreateHistory(string name)
        {
            if (ItemExists(name))
            {
                throw new DuplicateFileSystemHistoryException(name);
            }

            DoCreateHistory(name);
        }


        public abstract bool ItemExists(string name);

        protected abstract IFileSystemHistory DoGetHistory(string name);

        protected abstract void DoCreateHistory(string name);
    }
}
