// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitTransaction"/>
    /// </summary>
    public class GitTransactionTest : AbstractGitTransactionTest
    {

        [Fact(DisplayName = nameof(AbstractGitTransaction) + ".Begin() throws " + nameof(GitTransactionException) + " if the local directory exists and is not empty")]
        public void Begin_throws_GitTransactionException_if_local_directory_exists_and_is_not_empty()
        {
            var transaction = CreateTransaction();

            Directory.CreateDirectory(transaction.LocalPath);
            File.WriteAllText(Path.Combine(transaction.LocalPath, "file1"), "Irrelevant");

            Assert.Throws<GitTransactionException>(() => transaction.Begin());
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() deletes the local directory after pushing the changes")]
        public void Commit_deletes_the_local_directory_after_pushing_the_changes()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            AddFile(transaction, "master", "file1");
            transaction.Commit();

            Assert.False(Directory.Exists(transaction.LocalPath));
        }

        protected override IGitTransaction CreateTransaction()
        {
            return new GitTransaction(RemoteRepositoryPath, GetLocalTransactionDirectory());
        }

        protected override IGitTransaction CreateTransaction(string remotePath, string localPath)
        {
            return new GitTransaction(remotePath, localPath);
        }
    }
}