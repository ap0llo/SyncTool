using System;

namespace SyncTool.Git.RepositoryAccess.Transactions
{
    [Serializable]
    public class TransactionFailedException : GitTransactionException
    {
        public TransactionFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public TransactionFailedException(string message) : base(message)
        {
        }
    }
}