using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.FileSystem.Git.Utilities
{
    [Serializable]
    class RepositoryCloneException : Exception
    {

        public RepositoryCloneException(string message) : base(message)
        {
            
        }

    }
}
