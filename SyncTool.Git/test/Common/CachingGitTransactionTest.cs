// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using LibGit2Sharp;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="CachingGitTransaction"/>
    /// </summary>
    public class CachingGitTransactionTest : AbstractGitTransactionTest
    {

        #region Commit

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".Commit() does not delete the local directory if the transaction was successful")]
        
        public void Commit_does_not_delete_the_local_directory_if_the_transaction_was_successful()
        {
            var transaction = CreateTransaction();
            transaction.Begin();
            AddFile(transaction, "master", "file1");
            transaction.Commit();

            Assert.True(Directory.Exists(transaction.LocalPath));
            Assert.True(Repository.IsValid(transaction.LocalPath));
        }

        #endregion


        #region CanReuseLocalRepository

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the directory does not exist")]
        public void CanReuseLocalRepository_returns_false_if_the_directory_does_not_exist()
        {
            var transaction = CreateCachingTransaction();            
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the directory is empty")]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_empty()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the directory is not a git repository")]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_not_a_git_repository()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            File.WriteAllText(Path.Combine(transaction.LocalPath, "file1"), "Irrelevant");
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the directory is not a bare repository")]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_not_a_bare_repository()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Repository.Init(transaction.LocalPath);            
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the repository has no origin")]
        public void CanReuseLocalRepository_returns_false_if_the_repository_has_no_origin()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Repository.Init(transaction.LocalPath, true);
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the repository has a different origin")]
        public void CanReuseLocalRepository_returns_false_if_the_repository_has_a_different_origin()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Repository.Init(transaction.LocalPath, true);
            using (var repository = new Repository(transaction.LocalPath))
            {
                repository.Network.Remotes.Add("origin", "somePath");
            }
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the repository contains unpublished branches")]
        public void CanReuseLocalRepository_returns_false_if_the_repository_contains_unpublished_branches()
        {
            var transaction1 = CreateTransaction();
            transaction1.Begin();
            using (var repository = new Repository(transaction1.LocalPath))
            {
                repository.CreateBranch("newBranch", repository.GetAllCommits().Single());
            }

            //do not commit the changes from the transaction (leaves the local directory intact (with a new commit))


            var cachingTransaction = CreateCachingTransaction(transaction1.LocalPath);
            Assert.False(cachingTransaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() return false if the repository is ahead of origin")]
        public void CanReuseLocalRepository_returns_false_if_the_repository_is_ahead_of_origin()
        {
            var transaction1 = CreateTransaction();
            transaction1.Begin();
            AddFile(transaction1, "master", "file1");

            //do not commit the changes from the transaction (leaves the local directory intact (with a new commit))

        
            var cachingTransaction = CreateCachingTransaction(transaction1.LocalPath);                       
            Assert.False(cachingTransaction.CanReuseLocalRepository());
        }

        [Fact(DisplayName = nameof(CachingGitTransaction) + ".CanReuseLocalRepository() returns true for a repository left over from a successful transactioin")]
        public void CanReuseLocalRepository_returns_true_for_a_repository_left_over_from_a_successful_transaction()
        {
            var transaction1 = CreateTransaction();
            transaction1.Begin();
            AddFile(transaction1, "master", "file1");            
            transaction1.Commit();


            var transaction2 = CreateCachingTransaction(transaction1.LocalPath);
            Assert.True(transaction2.CanReuseLocalRepository());
        }


        #endregion



        protected override IGitTransaction CreateTransaction() => CreateCachingTransaction();
       
        CachingGitTransaction CreateCachingTransaction() => CreateCachingTransaction(GetLocalTransactionDirectory());

        CachingGitTransaction CreateCachingTransaction(string localPath)
        {
            return new CachingGitTransaction(RemoteRepositoryPath, localPath);
        }


    }
}