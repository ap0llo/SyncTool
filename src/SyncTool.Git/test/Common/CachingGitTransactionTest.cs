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

        [Fact]
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

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_directory_does_not_exist()
        {
            var transaction = CreateCachingTransaction();            
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_empty()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_not_a_git_repository()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            File.WriteAllText(Path.Combine(transaction.LocalPath, "file1"), "Irrelevant");
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_directory_is_not_a_bare_repository()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Repository.Init(transaction.LocalPath);            
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_repository_has_no_origin()
        {
            var transaction = CreateCachingTransaction();
            Directory.CreateDirectory(transaction.LocalPath);
            Repository.Init(transaction.LocalPath, true);
            Assert.False(transaction.CanReuseLocalRepository());
        }

        [Fact]
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

        [Fact]
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

        [Fact]
        public void CanReuseLocalRepository_returns_false_if_the_repository_is_ahead_of_origin()
        {
            var transaction1 = CreateTransaction();
            transaction1.Begin();
            AddFile(transaction1, "master", "file1");

            //do not commit the changes from the transaction (leaves the local directory intact (with a new commit))

        
            var cachingTransaction = CreateCachingTransaction(transaction1.LocalPath);                       
            Assert.False(cachingTransaction.CanReuseLocalRepository());
        }

        [Fact]
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


        #region Begin

        [Fact]
        public void Begin_gets_all_changes_from_the_remote_repository()
        {            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            // add a file to the repository in transaction1
            AddFile(transaction1, s_Branch2, "file1");
            transaction1.Commit();

            //in transaction2, add a file, too
            AddFile(transaction2, s_Branch3, "file2");
            transaction2.Commit();

            //create a new transaction using the directory of transaction1

            var transaction3 = CreateCachingTransaction(transaction1.LocalPath);
            // Begin() needs to fetch the changes made by transaction2 into the local repository
            transaction3.Begin();

            // make sure there are really all commits in the local repository
            using (var repository = new Repository(transaction3.LocalPath))
            {
                Assert.Equal(3, repository.GetAllCommits().Count());
            }
        }

        
        [Fact]
        public void Begin_creates_new_local_branches_for_branches_created_in_the_remote_repository()
        {
            const string branchName = "newTestBranch";

            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            // in transaction1 create a branch and push it to the remote directory
            using (var repository = new Repository(transaction1.LocalPath))
            {
                repository.CreateBranch(branchName, repository.Commits.Single());
            }            

            transaction1.Commit();

            var transaction3 = CreateCachingTransaction(transaction2.LocalPath);
            transaction3.Begin();

            using (var repository = new Repository(transaction3.LocalPath))
            {
                Assert.True(repository.Branches[branchName] != null);
            }                
        }

        
        [Fact(Skip = "Requires unreleased features in libgit2sharp")]
        public void Begin_deletes_local_branches_if_the_tracked_brach_was_deleted_in_the_remote_repository()
        {

            var transaction1 = CreateTransaction();            
            transaction1.Begin();

            m_RemoteRepository.Branches.Remove(s_Branch3);
            
            var transaction2 = CreateCachingTransaction(transaction1.LocalPath);
            transaction2.Begin();

            // assert that the branch has been removed in the local repository and that other branches are still there
            using (var repository = new Repository(transaction2.LocalPath))
            {
                Assert.False(repository.LocalBranchExists(s_Branch3));
                Assert.True(repository.LocalBranchExists(s_Branch2));
                Assert.True(repository.LocalBranchExists("master"));
                
            }
        }

        [Fact]
        public void Begin_sets_the_State_to_active_when_reusing_a_previous_repository()
        {
            var transaction1 = CreateTransaction();
            transaction1.Begin();

            //do not commit the changes from the transaction (leaves the local directory intact (with a new commit))        

            var cachingTransaction = CreateCachingTransaction(transaction1.LocalPath);
            cachingTransaction.Begin();

            Assert.Equal(TransactionState.Active, cachingTransaction.State);


        }

        #endregion



        protected override IGitTransaction CreateTransaction() => CreateCachingTransaction();

        protected override IGitTransaction CreateTransaction(string remotePath, string localPath) => CreateCachingTransaction(remotePath, localPath);

        CachingGitTransaction CreateCachingTransaction() => CreateCachingTransaction(GetLocalTransactionDirectory());

        CachingGitTransaction CreateCachingTransaction(string localPath)
        {
            return new CachingGitTransaction(RemoteRepositoryPath, localPath);
        }

        CachingGitTransaction CreateCachingTransaction(string remotePath, string localPath)
        {
            return new CachingGitTransaction(remotePath, localPath);
        }

    }
}