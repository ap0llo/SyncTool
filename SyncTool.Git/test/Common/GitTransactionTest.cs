// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using Xunit;

namespace SyncTool.Git.Common
{
    public class GitTransactionTest : AbstractGitTransactionTest
    {
        
        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() deletes the local directory after pushing the changes")]
        public void Commit_deletes_the_local_directory_after_pushing_the_changes()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            AddFile(transaction, "master", "file1");
            transaction.Commit();

            Assert.False(Directory.Exists(transaction.LocalPath));
        }

        protected override AbstractGitTransaction CreateTransaction()
        {
            return new GitTransaction(RemoteRepositoryPath, GetLocalTransactionDirectory());
        }
    }
}