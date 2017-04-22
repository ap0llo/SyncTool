using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Common
{
    public class RepositoryExtensionsTest : DirectoryBasedTest
    {

        Repository m_Repository;

        public RepositoryExtensionsTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);
            m_Repository = new Repository(m_TempDirectory.Location);
        }

        [Fact]
        public void IsCommitAncestor_returns_true_if_commit_is_ancestor()
        {
            string ancestorId;
            string descandantId;
                   
            // create 2 commits on master branch
            using (var workingDirectory = new TemporaryWorkingDirectory(m_TempDirectory.Location, "master"))
            {
                File.WriteAllText(Path.Combine(workingDirectory.Location, "file1.txt"), "Irrelevant");
                ancestorId = workingDirectory.Commit("Commit1");

                File.WriteAllText(Path.Combine(workingDirectory.Location, "file2.txt"), "Irrelevant");
                descandantId = workingDirectory.Commit("Commit2");

                workingDirectory.Push();                
            }

            Assert.True(m_Repository.IsCommitAncestor(ancestorId, descandantId));
        }


        [Fact]
        public void IsCommitAncestor_returns_false_if_commits_are_from_different_branches()
        {
            m_Repository.CreateBranch("branch1", m_Repository.Commits.Single());
            m_Repository.CreateBranch("branch2", m_Repository.Commits.Single());

            string ancestorId;
            using (var workingDirectory = new TemporaryWorkingDirectory(m_TempDirectory.Location, "branch1"))
            {
                File.WriteAllText(Path.Combine(workingDirectory.Location, "file1.txt"), "Irrelevant");
                ancestorId = workingDirectory.Commit("Commit1");                
                workingDirectory.Push();
            }
        
            string descandantId;
            using (var workingDirectory = new TemporaryWorkingDirectory(m_TempDirectory.Location, "branch2"))
            {
                File.WriteAllText(Path.Combine(workingDirectory.Location, "file1.txt"), "Irrelevant");
                descandantId = workingDirectory.Commit("Commit2");
                workingDirectory.Push();
            }
            
            Assert.False(m_Repository.IsCommitAncestor(ancestorId, descandantId));
            Assert.False(m_Repository.IsCommitAncestor(descandantId, ancestorId));
        }

        [Fact]
        public void IsCommitAncestor_returns_false_is_descendant_and_ancestor_are_swapped()
        {
            string ancestorId;
            string descandantId;

            // create 2 commits on master branch
            using (var workingDirectory = new TemporaryWorkingDirectory(m_TempDirectory.Location, "master"))
            {
                File.WriteAllText(Path.Combine(workingDirectory.Location, "file1.txt"), "Irrelevant");
                ancestorId = workingDirectory.Commit("Commit1");

                File.WriteAllText(Path.Combine(workingDirectory.Location, "file2.txt"), "Irrelevant");
                descandantId = workingDirectory.Commit("Commit1");

                workingDirectory.Push();
            }
            
            Assert.False(m_Repository.IsCommitAncestor(descandantId, ancestorId));
        }


        public override void Dispose()
        {
            m_Repository.Dispose();           
            base.Dispose();
        }
    }
}