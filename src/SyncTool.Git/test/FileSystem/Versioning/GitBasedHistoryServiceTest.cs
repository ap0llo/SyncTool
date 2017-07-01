using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Services;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
using SyncTool.FileSystem.Versioning;
using Xunit;

namespace SyncTool.Git.FileSystem.Versioning
{

    /// <summary>
    /// Tests for <see cref="GitBasedHistoryService"/>
    /// </summary>
    public class GitBasedHistoryServiceTest : GitGroupBasedTest
    {

        [Fact]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] { "history1", "histroy2" };

            using (var group = CreateGroup())
            {
                var historyService = group.GetHistoryService();
                foreach (var name in historyNames)
                {
                    historyService.CreateHistory(name);
                }

                Assert.Equal(historyNames.Length, historyService.Items.Count());
            }

        }

        [Theory]
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
                var historyService = group.GetHistoryService();

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

        [Fact]
        public void CreateHistory_throws_DuplicateFileSystemHistoryException_if_a_history_with_the_same_name_already_exists()
        {
            const string historyName = "history1";

            using(var group = CreateGroup())
            {
                var historyService = group.GetHistoryService();
                historyService.CreateHistory(historyName);

                Assert.Throws<DuplicateFileSystemHistoryException>(() => historyService.CreateHistory(historyName));
            }
        }


        [Fact]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            using (var group = CreateGroup())
            {
                var service = group.GetHistoryService();

                Assert.Throws<ArgumentNullException>(() => service[null]);
                Assert.Throws<ArgumentNullException>(() => service[""]);
                Assert.Throws<ArgumentNullException>(() => service[" "]);
            }

        }

        [Fact]
        public void Indexer_Get_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {
            using (var group = CreateGroup())
            {
                var service = group.GetHistoryService();

                Assert.Throws<ItemNotFoundException>(() => service["Irrelevant"]);
            }
        }

        [Fact]
        public void Indexer_Get_returns_expected_item()
        {
            using (var group = CreateGroup())
            {
                var service = group.GetHistoryService();

                service.CreateHistory("item1");

                Assert.NotNull(service["item1"]);
                Assert.NotNull(service["ITem1"]);

                // make sure the history has the name it was initially created with instead of the name it was retrieved with
                // otherwise there might be problem with pushing changes back to the master repository                
                Assert.Equal("item1", service["ITem1"].Name);
            }


        }


    }
}