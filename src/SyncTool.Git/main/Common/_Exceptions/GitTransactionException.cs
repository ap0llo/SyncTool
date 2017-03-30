using System;

namespace SyncTool.Git.Common
{
    [Serializable]
    public class GitTransactionException : Exception
    {

        public GitTransactionException(string message, Exception innerException) : base(message, innerException)
        {
            
        }

        public GitTransactionException(string message) : base(message)
        {
            
        }

    }
}
