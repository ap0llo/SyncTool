using System;

namespace SyncTool.Git.Common
{
    [Serializable]
    public class InvalidTransactionStateException : GitTransactionException
    {
        public InvalidTransactionStateException(TransactionState expectedState, TransactionState actualState) : base($"The transaction is in state '{actualState}' but was expected to be in state '{expectedState}'")
        {
        }
    }
}