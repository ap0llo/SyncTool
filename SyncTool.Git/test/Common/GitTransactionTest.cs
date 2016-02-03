// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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


        #region Begin()

        [Fact]
        public void Begin_throws_GitTransactionRepository_if_local_directory_exists_and_is_not_empty()
        {
            Directory.CreateDirectory(m_LocalRepositoryPath);
            File.WriteAllText(Path.Combine(m_LocalRepositoryPath, "file1"), "Irrelevant");

            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            Assert.Throws<GitTransactionException>(() => transaction.Begin());            
        }

        [Fact]
        public void Begin_succeeds_if_LocalPath_does_not_exist()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(m_LocalRepositoryPath));
            Assert.True(Repository.IsValid(m_LocalRepositoryPath));        
        }

        [Fact]
        public void Begin_succeeds_if_LocalPath_exists_but_is_empty()
        {
            Directory.CreateDirectory(m_LocalRepositoryPath);            

            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            Assert.NotEmpty(Directory.GetFileSystemEntries(m_LocalRepositoryPath));
            Assert.True(Repository.IsValid(m_LocalRepositoryPath));


        }
    
        [Fact]
        public void Begin_creates_a_bare_repository()
        {
            var transaction = new GitTransaction(m_RemoteRepositoryPath, m_LocalRepositoryPath);
            transaction.Begin();

            using (var repository = new Repository(m_LocalRepositoryPath))
            {
                Assert.True(repository.Info.IsBare);
            }
        }


        [Fact]       
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

        #endregion    


        #region Commit()

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

        #endregion


        public override void Dispose()
        {
            m_RemoteRepository.Dispose();
            base.Dispose();

        }
        
    }
}