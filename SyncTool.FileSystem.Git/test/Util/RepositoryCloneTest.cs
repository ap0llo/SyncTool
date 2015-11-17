// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.CodeDom;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LibGit2Sharp;
using SyncTool.FileSystem.Git.Util;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryCloneTest : DirectoryBasedTest
    {
        const string s_Branch2 = "branch2";
        const string s_Branch3 = "branch3";

        readonly string m_RemoteRepositoryPath;
        readonly string m_LocalRepositoryPath;
        readonly Repository m_RemoteRepository;



        public RepositoryCloneTest()
        {
            m_RemoteRepositoryPath = Path.Combine(m_TempDirectory.Location, "Remote");
            m_LocalRepositoryPath = Path.Combine(m_TempDirectory.Location, "Local");

            RepositoryInitHelper.InitializeRepository(m_RemoteRepositoryPath);
            m_RemoteRepository = new Repository(m_RemoteRepositoryPath);
            m_RemoteRepository.CreateBranch(s_Branch2, m_RemoteRepository.Commits.Single());
        }



        [Fact(DisplayName = nameof(RepositoryClone) + ": Constructor creates a new clone if target directory does not exist")]
        public void Constructor_creates_a_new_clone_if_target_directory_does_not_exist()
        {
            var repositoryClone = new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath);

            using (var localRepository = new Repository(m_LocalRepositoryPath))
            {
                Assert.Equal(2, localRepository.Branches.Count(b => b.IsRemote == false));
                Assert.Equal(m_RemoteRepository.Branches.Select(x => x.FriendlyName), localRepository.Branches.Where(b => b.IsRemote == false).Select(b => b.FriendlyName));
            }
        }

        [Fact(DisplayName = nameof(RepositoryClone) + ": Constructor throws RepositoryCloneException if local repository is not a bare repository")]        
        public void Constructor_throws_RepositoryCloneException_if_local_repository_is_not_a_bare_repository()
        {
            Repository.Clone(m_RemoteRepositoryPath, m_LocalRepositoryPath, new CloneOptions {IsBare = false});

            Assert.Throws<RepositoryCloneException>(() => new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath));
        }

        [Fact(DisplayName = nameof(RepositoryClone) + ": Constructor throws RepositoryCloneException if local repository is a clone of a different repository")]
        public void Constructor_throws_RepositoryCloneException_if_local_repository_if_not_a_clone_of_a_different_remote_repository()
        {
            var remoteRepositoryPath2 = Path.Combine(m_TempDirectory.Location, "Remote2");
            
            RepositoryInitHelper.InitializeRepository(remoteRepositoryPath2);

            var clone = new RepositoryClone(remoteRepositoryPath2, m_LocalRepositoryPath);

            // assert that exception is thrown
            Assert.Throws<RepositoryCloneException>(() => new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath));
        }

        [Fact(DisplayName = nameof(RepositoryClone) + ": Constructor throws RepositoryCloneException if local repository has no origin")]
        public void Constructor_throws_RepositoryCloneException_if_local_repository_if_local_repository_has_no_origin()
        {
            // create another repository in the local repository directory
            RepositoryInitHelper.InitializeRepository(m_LocalRepositoryPath);            

            // assert that exception is thrown
            Assert.Throws<RepositoryCloneException>(() => new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath));
        }

        [Fact(DisplayName = nameof(RepositoryClone) + ": Constructor fetches new branches from remote")]
        public void Constructor_fetches_new_branches_from_remote()
        {            
            // create local clone (should clone the remote repository)
            var clone = new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath);

            // create new branch in remote repository
            m_RemoteRepository.CreateBranch(s_Branch3, m_RemoteRepository.Commits.Single());


            clone = new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath);

            using (var localRepository = new Repository(m_LocalRepositoryPath))
            {
                Assert.Equal(3, localRepository.Branches.Count(b => b.IsRemote == false));
                Assert.Equal(m_RemoteRepository.Branches.Select(x => x.FriendlyName), localRepository.Branches.Where(b => b.IsRemote == false).Select(b => b.FriendlyName));
            }
        }


        [Fact(DisplayName = nameof(RepositoryClone) + ".Push() pushes changes from all branches to the remote repository")]
        public void Push_pushes_changes_from_all_branches_to_the_remote_repository()
        {            
            var clone = new RepositoryClone(m_RemoteRepositoryPath, m_LocalRepositoryPath);


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
            clone.Push();

            // check that the commit where pushed to the remote directory
            Assert.Equal(expectedCommitCount, m_RemoteRepository.GetAllCommits().Count());
            
        }

        //TODO: Constructor deletes local branches tracking non existing remote branches

        public override void Dispose()
        {
            m_RemoteRepository.Dispose();
            base.Dispose();

        }


        

    }
}