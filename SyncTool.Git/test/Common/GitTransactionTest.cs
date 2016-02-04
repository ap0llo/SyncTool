// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    /// <summary>
    /// Tests for <see cref="GitTransaction"/>
    /// </summary>
    public class GitTransactionTest : DirectoryBasedTest
    {
        const string s_Branch2 = "branch2";
        const string s_Branch3 = "branch3";

        readonly string m_RemoteRepositoryPath;
        readonly string m_LocalRepositoryPath;
        readonly Repository m_RemoteRepository;



        public GitTransactionTest()
        {
            m_RemoteRepositoryPath = Path.Combine(m_TempDirectory.Directory.Location, "Remote");
            m_LocalRepositoryPath = Path.Combine(m_TempDirectory.Directory.Location, "Local");

            RepositoryInitHelper.InitializeRepository(m_RemoteRepositoryPath);
            m_RemoteRepository = new Repository(m_RemoteRepositoryPath);
            m_RemoteRepository.CreateBranch(s_Branch2, m_RemoteRepository.Commits.Single());
        }


        [Fact(DisplayName = nameof(GitTransaction) + ": Initial state is 'Created'")]
        public void Initial_state_is_Created()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            Assert.Equal(TransactionState.Created, transaction.State);
        }

        #region Begin()

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() throws " + nameof(GitTransactionException) + " if the local directory exists and is not empty")]
        public void Begin_throws_GitTransactionException_if_local_directory_exists_and_is_not_empty()
        {
            Directory.CreateDirectory(m_LocalRepositoryPath);
            File.WriteAllText(Path.Combine(m_LocalRepositoryPath, "file1"), "Irrelevant");

            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            Assert.Throws<GitTransactionException>(() => transaction.Begin());            
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() succeeds if the local directory does not exists")]
        public void Begin_succeeds_if_the_local_directory_does_not_exist()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(m_LocalRepositoryPath));
            Assert.True(Repository.IsValid(m_LocalRepositoryPath));        
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() succeeds if the local directory exists but is empty")]
        public void Begin_succeeds_if_the_local_directory_exists_but_is_empty()
        {
            Directory.CreateDirectory(m_LocalRepositoryPath);            

            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(m_LocalRepositoryPath));
            Assert.True(Repository.IsValid(m_LocalRepositoryPath));


        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() creates a bare repository")]
        public void Begin_creates_a_bare_repository()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            using (var repository = new Repository(m_LocalRepositoryPath))
            {
                Assert.True(repository.Info.IsBare);
            }
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() creates local branches for all remote branches")]
        public void Begin_creates_local_branches_for_all_remote_branches()
        {
            Directory.CreateDirectory(m_LocalRepositoryPath);

            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            using (var localRepository = new Repository(m_LocalRepositoryPath))
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

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() sets state to 'Active'")]
        public void Begin_sets_State_to_active()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.Equal(TransactionState.Active, transaction.State);
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() throws " + nameof(InvalidTransactionStateException) +" if State is 'Active'")]
        public void Begin_throws_InvalidTransactionStateException_if_state_is_Active()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.Throws<InvalidTransactionStateException>(() => transaction.Begin());
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Begin() throws " + nameof(InvalidTransactionStateException) + " if State is 'Completed'")]
        public void Begin_throws_InvalidTransactionStateException_if_State_is_Completed()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();
            transaction.Commit();

            Assert.Throws<InvalidTransactionStateException>(() => transaction.Begin());
        }

        #endregion


        #region Commit()

        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() throws " + nameof(InvalidTransactionStateException) + " if State is not 'Created'")]
        public void Commit_throws_InvalidTransactionStateException_if_state_is_Created()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            
            Assert.Throws<InvalidTransactionStateException>(() => transaction.Commit());
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() throws " + nameof(InvalidTransactionStateException) + " if State is not 'Completed'")]
        public void Commit_throws_InvalidTransactionStateException_if_state_is_Completed()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();
            transaction.Commit();
            Assert.Throws<InvalidTransactionStateException>(() => transaction.Commit());
        }

        
        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() sets state to 'Completed'")]
        public void Commit_sets_state_to_Completed()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();
            transaction.Commit();

            Assert.Equal(TransactionState.Completed, transaction.State);
        }

        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() pushes changes from all branches to the remote repository")]
        public void Commit_pushes_changes_from_all_branches_to_the_remote_repository()
        {            
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            var expectedCommitCount = m_RemoteRepository.Commits.Count();

            // make change on master branch
            using (var workingDirectory = new TemporaryWorkingDirectory(m_LocalRepositoryPath, "master"))
            {
                using (System.IO.File.Create(Path.Combine(workingDirectory.Location, "file1"))) { }

                workingDirectory.Commit("Commit 2");
                workingDirectory.Push();

                expectedCommitCount += 1;
            }

            using (var localRepo = new Repository(m_LocalRepositoryPath))
            {
                Assert.Equal(expectedCommitCount, localRepo.GetAllCommits().Count());
            }


            // make change on branch2
            using (var workingDirectory = new TemporaryWorkingDirectory(m_LocalRepositoryPath, s_Branch2))
            {
                using (System.IO.File.Create(Path.Combine(workingDirectory.Location, "file2"))) { }

                workingDirectory.Commit("Commit 3");
                workingDirectory.Push();

                expectedCommitCount += 1;
            }

            using (var localRepo = new Repository(m_LocalRepositoryPath))
            {
                Assert.Equal(expectedCommitCount, localRepo.GetAllCommits().Count());
            }



            // push to remote repository
            transaction.Commit();

            // check that the commit where pushed to the remote directory
            Assert.Equal(expectedCommitCount, m_RemoteRepository.GetAllCommits().Count());
            
        }


        [Fact(DisplayName = nameof(GitTransaction) + ".Commit() throws " + nameof(TransactionAbortedException) + " if changes could not be pushed to remote repository")]
        public void Commit_throws_TransactionAbortedException_if_changes_could_not_be_pushed_to_remote_repository()
        {

            // create 2 transaction committing to the same branch
            var localRepositoryPath1 = m_LocalRepositoryPath;
            var localRepositoryPath2 = Path.Combine(m_TempDirectory.Directory.Location, "Local2");

            var transaction1 = new GitTransaction(m_RemoteRepositoryPath, localRepositoryPath1);
            var transaction2 = new GitTransaction(m_RemoteRepositoryPath, localRepositoryPath2);

            // start transactions
            transaction1.Begin();
            transaction2.Begin();


            // make change on master branch (transaction 1)
            using (var workingDirectory = new TemporaryWorkingDirectory(localRepositoryPath1, "master"))
            {
                using (File.Create(Path.Combine(workingDirectory.Location, "file1"))) { }

                workingDirectory.Commit("Commit in Transaction 1");
                workingDirectory.Push();                
            }


            // make changes on master branch (transaction 2)
            using (var workingDirectory = new TemporaryWorkingDirectory(localRepositoryPath2, "master"))
            {
                using (File.Create(Path.Combine(workingDirectory.Location, "file2"))) { }

                workingDirectory.Commit("Commit in Transaction 2");
                workingDirectory.Push();
            }

          
            // complete transaction 1 
            transaction1.Commit();

            // try to complete transaction 2 (should fail because transaction 1 made changes to the same branch)
            Assert.Throws<TransactionAbortedException>(() => transaction2.Commit());
        }


        //TODO: changes by 2 transaction on different branches => commit needs to succeed

        //TODO: transaction 1 changes branch1 and branch2, transaction 2 changes branch1 => commit needs to fail

        #endregion


        public override void Dispose()
        {
            m_RemoteRepository.Dispose();
            base.Dispose();

        }
        
    }
}