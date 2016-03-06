// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.Configuration;
using SyncTool.Configuration.Model;
using SyncTool.Git.Common;
using SyncTool.Git.TestHelpers;
using Xunit;

namespace SyncTool.Git.Configuration
{
    /// <summary>
    /// Tests for <see cref="GitBasedConfigurationService"/>
    /// </summary>
    public class GitBasedConfigurationServiceTest : GitGroupBasedTest
    {
      

        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ".Items is empty for new directory")]
        public void Items_is_empty_for_new_directory()
        {                        
            using (var group = CreateGroup())
            {
                var syncGroup = new GitBasedConfigurationService(group);
                Assert.Empty(syncGroup.Items);
            }            
        }



        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ".AddSyncGroup creates a new commit in the underlying repository")]
        public void AddSyncGroup_creates_a_new_commit_in_the_underlying_repository()
        {

            
            int previousCommitCount;
            using (var repo = new Repository(m_RemotePath))
            {
                previousCommitCount = repo.GetAllCommits().Count();
            }
            
            var syncFolder = new SyncFolder() { Name = "folder1", Path = "foo", Filter = null };

            
            using (var group = CreateGroup())
            {
                var service = new GitBasedConfigurationService(group);
                service.AddSyncFolder(syncFolder);

                Assert.Single(service.Items);
                Assert.Equal(syncFolder, service.Items.Single());
            }

            using (var repo = new Repository(m_RemotePath))
            {
                Assert.Equal(previousCommitCount + 1, repo.GetAllCommits().Count());
            }

        }

        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ".AddSyncGroup throws " + nameof(DuplicateSyncFolderException))]
        public void AddSyncGroup_throws_DuplicateSyncFolderException()
        {
            
            var syncFolder1 = new SyncFolder() { Name = "folder1", Path = "foo", Filter = null };
            var syncFolder2 = new SyncFolder() { Name = "folder1", Path = "bar", Filter = null };

            using (var group = CreateGroup())
            {
                var service = new GitBasedConfigurationService(group);
                service.AddSyncFolder(syncFolder1);
                Assert.Throws<DuplicateSyncFolderException>(() => service.AddSyncFolder(syncFolder2));
            }            
        }



        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ": Indexer.Get throws ArgumentNullException if name is null or whitespace")]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {            
            using (var group = CreateGroup())
            {
                var service = new GitBasedConfigurationService(group);
                Assert.Throws<ArgumentNullException>(() => service[null]);
                Assert.Throws<ArgumentNullException>(() => service[""]);
                Assert.Throws<ArgumentNullException>(() => service[" "]);
            }
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ": Indexer.Get throws ItemNotFoundException if the requested item was not found")]
        public void Indexer_Get_throws_ItemNotFoundException_if_the_requested_item_was_not_found()
        {
            
            using (var group = CreateGroup())
            {
                var service = new GitBasedConfigurationService(group);
                Assert.Throws<ItemNotFoundException>(() => service["SomeName"]);                
            }
        }

        [Fact(DisplayName = nameof(GitBasedConfigurationService) + ": Indexer.Get returns the expected Item")]
        public void Indexer_Get_returns_the_expected_Item()
        {
            
            using (var group = CreateGroup())
            {
                var service = new GitBasedConfigurationService(group);
                service.AddSyncFolder(new SyncFolder() { Name = "folder1"});
                Assert.NotNull(service["folder1"]);
                // name has to be treated case-invariant
                Assert.NotNull(service["foLDEr1"]);
            }
        }




      
    }
}