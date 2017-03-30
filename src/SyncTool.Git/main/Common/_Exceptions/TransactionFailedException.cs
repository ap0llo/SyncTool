// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

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