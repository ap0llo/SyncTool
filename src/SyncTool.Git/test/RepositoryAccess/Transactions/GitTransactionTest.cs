using System.IO;
using SyncTool.Git.RepositoryAccess.Transactions;
using Xunit;

namespace SyncTool.Git.Test.RepositoryAccess.Transactions
{
    /// <summary>
    /// Tests for <see cref="GitTransaction"/>
    /// </summary>
    public class GitTransactionTest : AbstractGitTransactionTest
    {

        [Fact]
        public void Begin_throws_GitTransactionException_if_local_directory_exists_and_is_not_empty()
        {
            var transaction = CreateTransaction();

            Directory.CreateDirectory(transaction.LocalPath);
            File.WriteAllText(Path.Combine(transaction.LocalPath, "file1"), "Irrelevant");

            Assert.Throws<GitTransactionException>(() => transaction.Begin());
        }

        [Fact]
        public void Commit_clears_the_local_directory_after_pushing_the_changes()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            AddFile(transaction, "master", "file1");
            transaction.Commit();

            Assert.Empty(Directory.EnumerateFileSystemEntries(transaction.LocalPath));
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