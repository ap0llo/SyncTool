using System;

namespace SyncTool.Git.Common
{
    [Serializable]
    class GitTransactionException : Exception
    {

        public GitTransactionException(string message) : base(message)
        {
            
        }

    }
}
