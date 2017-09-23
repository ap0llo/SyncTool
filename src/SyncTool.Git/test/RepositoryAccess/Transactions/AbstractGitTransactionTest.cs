using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.Git.RepositoryAccess;
using SyncTool.Git.RepositoryAccess.Transactions;
using Xunit;

namespace SyncTool.Git.Test.RepositoryAccess.Transactions
{
    /// <summary>
    /// Tests cases for both <see cref="GitTransaction"/> and <see cref="CachingGitTransaction"/>
    /// </summary>
    public abstract class AbstractGitTransactionTest : DirectoryBasedTest
    {
        protected const string s_Branch2 = "branch2";
        protected const string s_Branch3 = "branch3";

        protected string RemoteRepositoryPath { get; }         
        protected readonly Repository m_RemoteRepository;

        int m_TransactionCount;


        protected AbstractGitTransactionTest()
        {
            RemoteRepositoryPath = Path.Combine(m_TempDirectory.Directory.Location, "Remote");
            
            RepositoryInitHelper.InitializeRepository(RemoteRepositoryPath);
            
            m_RemoteRepository = new Repository(RemoteRepositoryPath);
            m_RemoteRepository.CreateBranch(s_Branch2, m_RemoteRepository.Commits.Single());
            m_RemoteRepository.CreateBranch(s_Branch3, m_RemoteRepository.Commits.Single());
        }


        [Fact]
        public void Initial_state_is_Created()
        {
            var transaction = CreateTransaction();
            Assert.Equal(TransactionState.Created, transaction.State);
        }

        #region Begin()



        [Fact]
        public void Begin_succeeds_if_the_local_directory_does_not_exist()
        {
            var transaction = CreateTransaction();
            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(transaction.LocalPath));
            Assert.True(Repository.IsValid(transaction.LocalPath));        
        }

        [Fact]
        public void Begin_succeeds_if_the_local_directory_exists_but_is_empty()
        {
            var transaction = CreateTransaction();

            Directory.CreateDirectory(transaction.LocalPath);

            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(transaction.LocalPath));
            Assert.True(Repository.IsValid(transaction.LocalPath));
        }

        [Fact]
        public void Begin_creates_a_bare_repository()
        {
            var transaction = CreateTransaction();
            transaction.Begin();

            using (var repository = new Repository(transaction.LocalPath))
            {
                Assert.True(repository.Info.IsBare);
            }
        }

        [Fact]
        public void Begin_creates_local_branches_for_all_remote_branches()
        {
            var transaction = CreateTransaction();

            Directory.CreateDirectory(transaction.LocalPath);

            transaction.Begin();

            using (var localRepository = new Repository(transaction.LocalPath))
            {
                Assert.Equal(m_RemoteRepository.Branches.Count(), localRepository.GetLocalBranches().Count());
                Assert.Equal(m_RemoteRepository.Branches.Select(x => x.FriendlyName), localRepository.GetLocalBranches().Select(b => b.FriendlyName));

                // make sure all local branches are set up to track the remote branches
                foreach (var branch in localRepository.GetLocalBranches())
                {
                    Assert.True(branch.IsTracking);
                    Assert.Equal("origin/" + branch.FriendlyName, branch.TrackedBranch.FriendlyName);
                }               
            }
        }

        [Fact]
        public void Begin_sets_State_to_active()
        {
            var transaction = CreateTransaction();
            transaction.Begin();
            Assert.Equal(TransactionState.Active, transaction.State);
        }

        [Fact]
        public void Begin_throws_InvalidTransactionStateException_if_state_is_Active()
        {
            var transaction = CreateTransaction();
            transaction.Begin();
            Assert.Throws<InvalidTransactionStateException>(() => transaction.Begin());
        }

        [Fact]
        public void Begin_throws_InvalidTransactionStateException_if_State_is_Completed()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            transaction.Commit();

            Assert.Throws<InvalidTransactionStateException>(() => transaction.Begin());
        }

        [Fact]
        public void Begin_throws_InvalidTransactionStateException_if_State_is_Failed()
        {
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            AddFile(transaction1, "master", "file1");
            AddFile(transaction2, "master", "file2");

            transaction1.Commit();
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());
            
            Assert.Throws<InvalidTransactionStateException>(() => transaction1.Begin());
            Assert.Equal(TransactionState.Failed, transaction2.State);
            Assert.Throws<InvalidTransactionStateException>(() => transaction2.Begin());
        }

        [Fact]
        public void Begin_throws_TransactionCloneException_if_RemotePath_is_not_a_valid_git_repository()
        {
            var transaction = CreateTransaction(GetLocalTransactionDirectory(), GetLocalTransactionDirectory());
            Assert.Throws<TransactionCloneException>(() => transaction.Begin());
        }

        #endregion


        #region Commit()

        [Fact]
        public void Commit_throws_InvalidTransactionStateException_if_state_is_Created()
        {
            var transaction = CreateTransaction();
            Assert.Throws<InvalidTransactionStateException>(() => transaction.Commit());
        }

        [Fact]
        public void Commit_throws_InvalidTransactionStateException_if_state_is_Completed()
        {
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            AddFile(transaction1, "master", "file1");
            AddFile(transaction2, "master", "file2");

            transaction1.Commit();
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());

            Assert.Throws<InvalidTransactionStateException>(() => transaction1.Commit());
            Assert.Equal(TransactionState.Failed, transaction2.State);
            Assert.Throws<InvalidTransactionStateException>(() => transaction2.Commit());
        }

        [Fact]
        public void Commit_throws_InvalidTransactionStateException_if_state_is_Failed()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            transaction.Commit();

            Assert.Throws<InvalidTransactionStateException>(() => transaction.Commit());
        }

        [Fact]
        public void Commit_sets_state_to_Completed_when_successful()
        {
            var transaction = CreateTransaction();

            transaction.Begin();
            transaction.Commit();

            Assert.Equal(TransactionState.Completed, transaction.State);
        }

        [Fact]
        public void Commit_sets_state_to_Completed_when_transaction_failed()
        {
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            AddFile(transaction1, "master", "file1");
            AddFile(transaction2, "master", "file2");

            transaction1.Commit();
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());
            Assert.Equal(TransactionState.Failed, transaction2.State);
        }

        [Fact]
        public void Commit_pushes_changes_from_all_branches_to_the_remote_repository()
        {
            var transaction = CreateTransaction();
            transaction.Begin();

            var expectedCommitCount = m_RemoteRepository.Commits.Count();

            // make change on master branch
            AddFile(transaction, "master", "file1");            
            expectedCommitCount += 1;
            
            using (var localRepo = new Repository(transaction.LocalPath))
            {
                Assert.Equal(expectedCommitCount, localRepo.GetAllCommits().Count());
            }


            // make change on branch2
            AddFile(transaction, s_Branch2, "file2");            
            expectedCommitCount += 1;

            using (var localRepo = new Repository(transaction.LocalPath))
            {
                Assert.Equal(expectedCommitCount, localRepo.GetAllCommits().Count());
            }

            // push to remote repository
            transaction.Commit();

            // check that the commit where pushed to the remote directory
            Assert.Equal(expectedCommitCount, m_RemoteRepository.GetAllCommits().Count());            
        }

        [Fact]
        public void Commit_throws_TransactionFailedException_if_changes_could_not_be_pushed_to_remote_repository()
        {
            // create 2 transaction committing to the same branch            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            // start transactions
            transaction1.Begin();
            transaction2.Begin();

            // make change on master branch (transaction 1)
            AddFile(transaction1, "master", "file1");
            
            // make changes on master branch (transaction 2)
            AddFile(transaction2, "master", "file2");
                      
            // complete transaction 1 
            transaction1.Commit();

            // try to complete transaction 2 (should fail because transaction 1 made changes to the same branch)
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());
        }

        [Fact]
        public void Commit_succeeds_if_transactions_work_on_different_branches()
        {          
            // create 2 transaction committing to the same branch
            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            // start transactions
            transaction1.Begin();
            transaction2.Begin();

            // make change on master branch (transaction 1)
            AddFile(transaction1, s_Branch2, "file1");
            
            // make changes on master branch (transaction 2)
            AddFile(transaction2, s_Branch3, "file2");
            
            transaction1.Commit();
            transaction2.Commit();

            // make sure all the commits made it to the remote repository
            Assert.Equal(3, m_RemoteRepository.GetAllCommits().Count());            
        }

        [Fact]
        public void Commit_pushes_newly_created_branches()
        {
            var branchName = "newBranch";

            var transaction = CreateTransaction();
            transaction.Begin();

            using (var repository = new Repository(transaction.LocalPath))
            {
                repository.CreateBranch(branchName, repository.Commits.Single());
            }

            AddFile(transaction, branchName, "file1");
            
            transaction.Commit();

            Assert.Contains(m_RemoteRepository.Branches, x => x.FriendlyName == branchName);
            Assert.Equal(2, m_RemoteRepository.GetAllCommits().Count());
        }
        
        [Fact]
        public void Commit_fails_to_create_a_new_branch_if_the_same_branch_was_created_by_another_transaction()
        {
            const string branchName = "newBranch";
            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            using (var localRepository1 = new Repository(transaction1.LocalPath))
            {
                localRepository1.CreateBranch(branchName, localRepository1.Commits.Single());
            }

            AddFile(transaction1, branchName, "file1");

            using (var localRepository2 = new Repository(transaction2.LocalPath))
            {
                localRepository2.CreateBranch(branchName, localRepository2.Commits.Single());
            }

            transaction1.Commit();
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());
        }

        [Fact]
        public void Commit_succeeds_if_two_transactions_created_the_same_branch_pointing_to_the_same_commit()
        {
            const string branchName = "newBranch";
            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            // transaction 1: create branch
            using (var localRepository1 = new Repository(transaction1.LocalPath))
            {
                localRepository1.CreateBranch(branchName, localRepository1.Commits.Single());
            }

            // transaction 2: create branch
            using (var localRepository2 = new Repository(transaction2.LocalPath))
            {
                localRepository2.CreateBranch(branchName, localRepository2.Commits.Single());
            }

            // commit both transactions
            transaction1.Commit();
            transaction2.Commit();

            Assert.Contains(m_RemoteRepository.Branches, x => x.FriendlyName == branchName);
        }

        [Fact]
        public void Commit_fails_when_only_one_branch_was_changed_on_the_remote()
        {            
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();

            transaction1.Begin();
            transaction2.Begin();

            // transaction 1: create commit on branch2 and branch3
            AddFile(transaction1, s_Branch2, "file1");
            AddFile(transaction1, s_Branch3, "file2");
            
            // transaction 2: create commit on branch2
            AddFile(transaction2, s_Branch2, "file3");
            
            // transaction 2 commit its changes first
            transaction2.Commit();

            // transaction 1 needs to fail
            Assert.Throws<TransactionFailedException>(() => transaction1.Commit());

            // check that no commit from transaction 2 made it to the master repository 
            // expected 2 commits (one initial commit created when initializing the repo and two created by transaction 1)
            Assert.Equal(2, m_RemoteRepository.GetAllCommits().Count());
        }

        [Fact]
        public void Commit_deletes_the_local_directory_even_when_the_transaction_fails()
        {
            var transaction1 = CreateTransaction();
            var transaction2 = CreateTransaction();
                        
            transaction1.Begin();
            transaction2.Begin();

            AddFile(transaction1, "master", "file1");
            AddFile(transaction2, "master", "file2");
            
            transaction1.Commit();
            Assert.Throws<TransactionFailedException>(() => transaction2.Commit());            
            
            Assert.False(Directory.Exists(transaction2.LocalPath));
        }


        [Fact]
        public void Commit_pushes_tags_created_within_the_transaction()
        {
            const string tagName = "testTag";

            var transaction = CreateTransaction();
            transaction.Begin();

            using (var repository = new Repository(transaction.LocalPath))
            {
                repository.Tags.Add(tagName, repository.Commits.Single());
            }

            transaction.Commit();

            Assert.NotNull(m_RemoteRepository.Tags[tagName]);
            Assert.Equal(m_RemoteRepository.Commits.Single().Sha, m_RemoteRepository.Tags[tagName].Target.Sha);
        }

        #endregion
        

        public override void Dispose()
        {
            m_RemoteRepository.Dispose();
            base.Dispose();

        }

        /// <summary>
        /// Adds the specified file to the specified branch using the path of the transaction's local repository
        /// </summary>
        protected void AddFile(IGitTransaction transaction, string branchName, string fileName)
        {
            using (var workingDirectory = new TemporaryWorkingDirectory(null, transaction.LocalPath, branchName))
            {
                var path = Path.Combine(workingDirectory.Location, fileName);
                File.WriteAllText(path, "Some file content");

                workingDirectory.Commit();
                workingDirectory.Push();                
            }
        }

        protected abstract IGitTransaction CreateTransaction();

        protected abstract IGitTransaction CreateTransaction(string remotePath, string localPath);


        protected string GetLocalTransactionDirectory()
        {
            var localPath = Path.Combine(m_TempDirectory.Location, "Local" + m_TransactionCount);
            m_TransactionCount += 1;

            return localPath;
        }
        
        
    }
}