// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Git.TestHelpers;
using Xunit;

namespace SyncTool.Configuration.Git
{
    public class GitBasedSyncGroupTest : DirectoryBasedTest
    {

        [Fact(DisplayName = nameof(GitBasedSyncGroup) + ".Folders is empty for new directory")]
        public void Folders_is_empty_for_new_directory()
        {            
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, "Irrelevant");

            using (var syncGroup = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                Assert.Empty(syncGroup.Folders);
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroup) + ".Name returns name specified in RepositoryInfo file")]
        public void Name_returns_name_specified_in_RepositoryInfo_file()
        {
            var name = Guid.NewGuid().ToString();

            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, name);

            using (var syncGroup = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                Assert.Equal(name, syncGroup.Name);
            }
        }

        [Fact(DisplayName = nameof(GitBasedSyncGroup) + ".AddSyncGroup creates a new commit in the underlying repository")]
        public void AddSyncGroup_creates_a_new_commit_in_the_underlying_repository()
        {

            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, "Irrelevant");

            var previousCommitCount = 0;
            using (var repo = new Repository(m_TempDirectory.Location))
            {
                previousCommitCount = repo.GetAllCommits().Count();
            }
            
            var syncFolder = new SyncFolder() { Name = "folder1", Path = "foo", ReadFilter = null };

            using (var group = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                group.AddSyncFolder(syncFolder);

                Assert.Single(group.Folders);
                Assert.Equal(syncFolder, group.Folders.Single());
            }

            using (var repo = new Repository(m_TempDirectory.Location))
            {
                Assert.Equal(previousCommitCount + 1, repo.GetAllCommits().Count());
            }

        }

        [Fact(DisplayName = nameof(GitBasedSyncGroup) + ".AddSyncGroup throws " + nameof(DuplicateSyncFolderException))]
        public void AddSyncGroup_throws_DuplicateSyncFolderException()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location, "Irrelevant");            

            var syncFolder1 = new SyncFolder() { Name = "folder1", Path = "foo", ReadFilter = null };
            var syncFolder2 = new SyncFolder() { Name = "folder1", Path = "bar", ReadFilter = null };

            using (var group = new GitBasedSyncGroup(m_TempDirectory.Location))
            {
                group.AddSyncFolder(syncFolder1);
                Assert.Throws<DuplicateSyncFolderException>(() => group.AddSyncFolder(syncFolder2));
            }            
        }



    }
}