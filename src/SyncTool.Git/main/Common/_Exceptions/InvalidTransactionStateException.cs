// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

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