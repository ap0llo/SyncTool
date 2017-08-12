using SyncTool.Common.Services;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using SyncTool.Sql.Services;
using SyncTool.Sql.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SyncTool.Sql.Test.Services
{
    /// <summary>
    /// Tests for <see cref="SqlHistoryService"/>
    /// </summary>
    public class SqlHistoryServiceTest : SqlTestBase
    {

        SqlHistoryService CreateInstance() => new SqlHistoryService(
            new FileSystemHistoryRepository(ContextFactory), 
            historyDo => new SqlFileSystemHistory(ContextFactory, (_,__) => null, historyDo));

        [Fact]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] { "history1", "histroy2" };

            var historyService = CreateInstance();
            foreach (var name in historyNames)
            {
                historyService.CreateHistory(name);
            }

            Assert.Equal(historyNames.Length, historyService.Items.Count());

        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void CreateHistory_creates_entries_in_the_database(int numberOfHistoriesToCreate)
        {
            var historyService = CreateInstance();
            for (var i = 0; i < numberOfHistoriesToCreate; i++)
            {
                historyService.CreateHistory($"history-{i}");
            }

            using (var connection = ContextFactory.OpenConnection())
            {
                var count = connection.ExecuteScalar<int>("SELECT count(*) FROM FileSystemHistories");
                Assert.Equal(numberOfHistoriesToCreate, count);            
            }
        }

        [Fact]
        public void CreateHistory_throws_DuplicateFileSystemHistoryException_if_a_history_with_the_same_name_already_exists()
        {
            const string historyName = "history1";
            
            var historyService = CreateInstance();
            historyService.CreateHistory(historyName);

            Assert.Throws<DuplicateFileSystemHistoryException>(() => historyService.CreateHistory(historyName));
        }    

        [Fact]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {            
            var service = CreateInstance();

            Assert.Throws<ArgumentNullException>(() => service[null]);
            Assert.Throws<ArgumentNullException>(() => service[""]);
        }

        [Fact]
        public void Indexer_Get_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {            
            var service = CreateInstance();
            Assert.Throws<ItemNotFoundException>(() => service["Irrelevant"]);
        }

        [Fact]
        public void Indexer_Get_returns_expected_item()
        {            
            var service = CreateInstance();

            service.CreateHistory("item1");

            Assert.NotNull(service["item1"]);
            Assert.NotNull(service["ITem1"]);

            // make sure the history has the name it was initially created with instead of the name it was retrieved with
            // otherwise there might be problem with pushing changes back to the master repository                
            Assert.Equal("item1", service["ITem1"].Name);
        }

    }
}
