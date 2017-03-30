// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Git.Common
{
    public enum TransactionState
    {
        /// <summary>
        /// The transaction has been created but is not yet active or completed
        /// </summary>
        Created = 0,

        /// <summary>
        /// The transaction is active, changes can be made
        /// </summary>
        Active = 1,

        /// <summary>
        /// The transaction has been completed successfully
        /// </summary>
        Completed = 2,

        /// <summary>
        /// The transaction has been completed but no changes were persisted
        /// </summary>
        Failed = 3
    }
}