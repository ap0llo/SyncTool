using System;

namespace SyncTool.Git.RepositoryAccess.Transactions
{
    [Serializable]
    public class InvalidTransactionStateException : GitTransactionException
    {
        public InvalidTransactionStateException(TransactionState expectedState, TransactionState actualState) : base($"The transaction is in state '{actualState}' but was expected to be in state '{expectedState}'")
        {
        }
    }
}