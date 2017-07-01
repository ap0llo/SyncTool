using System;

namespace SyncTool.Git.RepositoryAccess.Transactions
{
    public class TransactionCloneException : GitTransactionException
    {
        public TransactionCloneException(string message, Exception innerException) : base(message, innerException)
        {

        }

        public TransactionCloneException(string message) : base(message)
        {

        }
    }
}