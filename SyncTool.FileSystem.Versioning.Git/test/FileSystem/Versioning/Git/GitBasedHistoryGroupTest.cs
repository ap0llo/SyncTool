// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning.Git
{

    /// <summary>
    /// Tests for <see cref="GitBasedHistoryGroup"/>
    /// </summary>
    public class GitBasedHistoryGroupTest : DirectoryBasedTest
    {

        [Fact(DisplayName= "GitBasedHistoryRepository.Create() can create a new repository")]
        public void Create_can_create_a_new_repository()
        {
            using (var historyRepository = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {
                Assert.Empty(historyRepository.Items);                
            }
        }

        [Fact(DisplayName = "GitBasedHistoryRepository.CreateHistory() can create multiple histories")]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] {"history1", "histroy2"};

            using (var historyRepository = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {

                foreach (var name in historyNames)
                {
                    historyRepository.CreateHistory(name);
                }

                Assert.Equal(historyNames.Length, historyRepository.Items.Count());
            }
        }

        [Theory(DisplayName = "GitBasedHistoryRepository.CreateHistory() creates a new branch in the underlying git repository")]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void CreateHistory_creates_a_new_branch_in_the_underlying_git_repository(int numberOfHistoriesToCreate)
        {
            int initialBranchCount;
            
            // create the specified number of histories
            using (var historyRepository = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {
                // get number of branches in the repository before creating the histories
                using (var repo = new Repository(m_TempDirectory.Location))
                {
                    initialBranchCount = repo.Branches.Count();
                }

                for (int i = 0; i < numberOfHistoriesToCreate; i++)
                {
                    historyRepository.CreateHistory($"history-{i}");
                }           
            }

            // Assert that the expected number of branches has been created
            using (var repo = new Repository(m_TempDirectory.Location))
            {
                Assert.Equal(numberOfHistoriesToCreate + initialBranchCount, repo.Branches.Count());                
            }

        }


        [Fact]
        public void GetItem_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            using (var group = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {
                Assert.Throws<ArgumentNullException>(() => group.GetItem(null));
                Assert.Throws<ArgumentNullException>(() => group.GetItem(""));
                Assert.Throws<ArgumentNullException>(() => group.GetItem(" "));
            }
        }

        [Fact]
        public void GetItem_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {
            using (var group = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {
                Assert.Throws<ItemNotFoundException>(() => group.GetItem("Irrelevant"));                
            }
        }


        [Fact]
        public void GetItem_returns_expected_item()
        {
            using (var group = GitBasedHistoryGroup.Create(m_TempDirectory.Location))
            {
                group.CreateHistory("item1");
                
                Assert.NotNull(group.GetItem("item1"));
                Assert.NotNull(group.GetItem("ITem1"));

                // make sure the history has the name it was initially created with instead of the name it was retrieved with
                // otherwise there might be problem with pushing changes back to the master repository
                Assert.EndsWith("item1", group.GetItem("ITem1").Id);

            }
        }
    }
}