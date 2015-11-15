// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInitHelperTest : IDisposable
    {
        readonly TemporaryLocalDirectory m_TemporaryDirectory;

        public RepositoryInitHelperTest()
        {
            m_TemporaryDirectory = new LocalItemCreator().CreateTemporaryDirectory();
        }


        [Fact(DisplayName = nameof(RepositoryInitHelper) + ".InitializeRepository() creates a bare repository")]
        public void InitializeRepository_Creates_a_bare_repository()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // test if we can open the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {                
                Assert.True(repository.Info.IsBare);
            }
        }

        [Fact(DisplayName = nameof(RepositoryInitHelper) + ".InitializeRepository() creates an initial commit")]
        public void InitializeRepository_Creates_an_initial_commit()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                Assert.Single(repository.Commits);
            }
        }

        [Fact(DisplayName = nameof(RepositoryInitHelper) + ".InitializeRepository() adds a tag for the initial commit")]
        public void InitializeRepository_Adds_a_tag_for_the_initial_commit()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                var initialCommit = repository.Commits.Single();

                Assert.Single(repository.Tags);

                var tag = repository.Tags.Single();

                Assert.Equal(initialCommit.Sha, tag.Target.Sha);
                Assert.Equal(RepositoryInitHelper.InitialCommitTagName, tag.Name);
            }
        }

        [Fact(DisplayName = nameof(RepositoryInitHelper) + ".InitializeRepository() adds a repository info file to the repository")]
        public void InitializeRepository_Adds_a_repository_info_file_to_the_repository()
        {
            RepositoryInitHelper.InitializeRepository(m_TemporaryDirectory.Location);

            // assert that a single commit has been created in the repository
            using (var repository = new Repository(m_TemporaryDirectory.Location))
            {
                var initialCommit = repository.Commits.Single();
                
                var gitDirectory = new GitDirectory("Irrelevant", initialCommit);

                Assert.True(gitDirectory.FileExists(RepositoryInfoFile.RepositoryInfoFileName));

            }
        }


        public void Dispose()
        {
            m_TemporaryDirectory.Dispose();
        }
    }
}