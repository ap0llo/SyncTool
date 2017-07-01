using System;

namespace SyncTool.Git.RepositoryAccess.Transactions
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
