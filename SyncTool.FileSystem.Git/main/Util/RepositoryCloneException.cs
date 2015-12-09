using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.FileSystem.Git.Util
{
    [Serializable]
    class RepositoryCloneException : Exception
    {

        public RepositoryCloneException(string message) : base(message)
        {
            
        }

    }
}
