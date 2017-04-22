using System;

namespace SyncTool.Git.Common
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