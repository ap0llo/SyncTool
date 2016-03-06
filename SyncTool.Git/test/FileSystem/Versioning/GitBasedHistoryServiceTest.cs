// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.TestHelpers;
using Xunit;

namespace SyncTool.Git.FileSystem.Versioning
{

    /// <summary>
    /// Tests for <see cref="GitBasedHistoryService"/>
    /// </summary>
    public class GitBasedHistoryServiceTest : GitGroupBasedTest
    {
      


        [Fact(DisplayName = nameof(GitBasedHistoryService) + ".CreateHistory() can create multiple histories")]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] { "history1", "histroy2" };

            using (var group = CreateGroup())
            {               
                var historyService = new GitBasedHistoryService(group);
                foreach (var name in historyNames)
                {
                    historyService.CreateHistory(name);
                }

            Assert.Equal(historyNames.Length, historyService.Items.Count());
            }

        }

        [Theory(DisplayName = nameof(GitBasedHistoryService) + ".CreateHistory() creates a new branch in the underlying git repository")]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void CreateHistory_creates_a_new_branch_in_the_underlying_git_repository(int numberOfHistoriesToCreate)
        {
            int initialBranchCount;
            
            // get number of branches in the repository before creating the histories
            using (var repo = new Repository(m_RemotePath))
            {
                initialBranchCount = repo.Branches.Count();
            }

            using (var group = CreateGroup())
            {
                var historyService = new GitBasedHistoryService(group);

                for (var i = 0; i < numberOfHistoriesToCreate; i++)
                {
                    historyService.CreateHistory($"history-{i}");
                }
                
            }
            // Assert that the expected number of branches has been created
            using (var repo = new Repository(m_RemotePath))
            {
                Assert.Equal(numberOfHistoriesToCreate + initialBranchCount, repo.Branches.Count());
            }

        }

        [Fact(DisplayName = nameof(GitBasedHistoryService) + ".CreateHistory() throws " + nameof(DuplicateFileSystemHistoryException) + "if a history with the same name already exists")]
        public void CreateHistory_throws_DuplicateFileSystemHistoryException_if_a_history_with_the_same_name_already_exists()
        {
            const string historyName = "history1";

            using(var group = CreateGroup())
            {
                var historyService = new GitBasedHistoryService(group);
                historyService.CreateHistory(historyName);

                Assert.Throws<DuplicateFileSystemHistoryException>(() => historyService.CreateHistory(historyName));
            }
        }


        [Fact(DisplayName = nameof(GitBasedHistoryService) + ": Indexer.Get throws ArgumentNullException if name is null or whitespace")]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            using (var group = CreateGroup())
            {
                var service = new GitBasedHistoryService(group);

                Assert.Throws<ArgumentNullException>(() => service[null]);
                Assert.Throws<ArgumentNullException>(() => service[""]);
                Assert.Throws<ArgumentNullException>(() => service[" "]);
            }

        }

        [Fact(DisplayName = nameof(GitBasedHistoryService) + ": Indexer.Get throws ItemNotFoundException if requested item could not be found")]
        public void Indexer_Get_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {
            using (var group = CreateGroup())
            {
                var service = new GitBasedHistoryService(group);

                Assert.Throws<ItemNotFoundException>(() => service["Irrelevant"]);
            }
        }

        [Fact(DisplayName = nameof(GitBasedHistoryService) + ": Indexer.Get returns expected item")]
        public void Indexer_Get_returns_expected_item()
        {
            using (var group = CreateGroup())
            {
                var service = new GitBasedHistoryService(group);

                service.CreateHistory("item1");

                Assert.NotNull(service["item1"]);
                Assert.NotNull(service["ITem1"]);

                // make sure the history has the name it was initially created with instead of the name it was retrieved with
                // otherwise there might be problem with pushing changes back to the master repository
                Assert.EndsWith("item1", service["ITem1"].Id);
            }


        }


    }
}