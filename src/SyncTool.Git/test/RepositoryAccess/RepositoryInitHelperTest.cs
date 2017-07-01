using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using SyncTool.Git.FileSystem;
using SyncTool.Git.RepositoryAccess;
using Xunit;

namespace SyncTool.Git.Test.RepositoryAccess
{
    /// <summary>
    /// Tests for <see cref="RepositoryInitHelper"/>
    /// </summary>
    public sealed class RepositoryInitHelperTest : IDisposable
    {
        readonly DisposableLocalDirectoryWrapper m_TemporaryDirectory;

        public RepositoryInitHelperTest()
        {
            m_TemporaryDirectory = new LocalItemCreator().CreateTemporaryDirectory();
        }


        [Fact]
        public void InitializeRepository_Creates_a_bare_repository()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Directory.Location);

            // test if we can open the repository
            using (var repository = new Repository(m_TemporaryDirectory.Directory.Location))
            {                
                Assert.True(repository.Info.IsBare);
            }
        }

        [Fact]
        public void InitializeRepository_Creates_an_initial_commit()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Directory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                Assert.Single(repository.Commits);
            }
        }

        [Fact]
        public void InitializeRepository_Adds_a_tag_for_the_initial_commit()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                var initialCommit = repository.GetAllCommits().Single();

                Assert.Single(repository.Tags);

                var tag = repository.Tags.Single();

                Assert.Equal(initialCommit.Sha, tag.Target.Sha);
                Assert.Equal(RepositoryInitHelper.InitialCommitTagName, tag.FriendlyName);
            }
        }

        [Fact]
        public void InitializeRepository_Adds_a_repository_info_file_to_the_repository()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                var initialCommit = repository.Commits.Single();
                
                var gitDirectory = new GitDirectory(null, "Irrelevant", initialCommit);

                Assert.True(gitDirectory.FileExists(RepositoryInfoFile.RepositoryInfoFileName));

            }
        }

        [Fact]
        public void InitializeRepository_Creates_a_configuration_branch()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that the configuration branch has been created and points to the inital commit
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                var configurationBranch = repository.GetBranch(RepositoryInitHelper.ConfigurationBranchName);
                Assert.NotNull(configurationBranch);
                Assert.Single(configurationBranch.Commits);

                var initialCommitSha = repository.Tags[RepositoryInitHelper.InitialCommitTagName].Target.Sha;
                Assert.Equal(initialCommitSha, configurationBranch.Commits.Single().Sha);                
            }

        }


        public void Dispose()
        {
            m_TemporaryDirectory.Dispose();
        }
    }
}