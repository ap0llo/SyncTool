using System;

namespace SyncTool.Git.Common
{
    [Serializable]
    class RepositoryCloneException : Exception
    {

        public RepositoryCloneException(string message) : base(message)
        {
            
        }

    }
}
