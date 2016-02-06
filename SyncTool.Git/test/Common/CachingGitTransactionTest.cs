// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Git.Common
{
    public class CachingGitTransactionTest : AbstractGitTransactionTest
    {
        protected override AbstractGitTransaction CreateTransaction()
        {
            return new CachingGitTransaction(RemoteRepositoryPath, GetLocalTransactionDirectory());
        }
    }
}