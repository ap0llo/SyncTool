﻿using SyncTool.Common.Services;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;
using SyncTool.Sql.Services;
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
    public class SqlHistoryServiceTest : IDisposable
    {

        readonly DatabaseContext m_Context;

        public SqlHistoryServiceTest()
        {
            m_Context = new InMemoryDatabaseContext();
        }

        public void Dispose() => m_Context.Dispose();


        [Fact]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] { "history1", "histroy2" };

            var historyService = new SqlHistoryService(m_Context);
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
            var historyService = new SqlHistoryService(m_Context);
            for (var i = 0; i < numberOfHistoriesToCreate; i++)
            {
                historyService.CreateHistory($"history-{i}");
            }
            
            Assert.Equal(numberOfHistoriesToCreate, m_Context.FileSystemHistories.Count());            
        }

        [Fact]
        public void CreateHistory_throws_DuplicateFileSystemHistoryException_if_a_history_with_the_same_name_already_exists()
        {
            const string historyName = "history1";
            
            var historyService = new SqlHistoryService(m_Context);
            historyService.CreateHistory(historyName);

            Assert.Throws<DuplicateFileSystemHistoryException>(() => historyService.CreateHistory(historyName));
        }


        [Fact]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {            
            var service = new SqlHistoryService(m_Context);

            Assert.Throws<ArgumentNullException>(() => service[null]);
            Assert.Throws<ArgumentNullException>(() => service[""]);
        }

        [Fact]
        public void Indexer_Get_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {            
            var service = new SqlHistoryService(m_Context);
            Assert.Throws<ItemNotFoundException>(() => service["Irrelevant"]);
        }

        [Fact]
        public void Indexer_Get_returns_expected_item()
        {            
            var service = new SqlHistoryService(m_Context);

            service.CreateHistory("item1");

            Assert.NotNull(service["item1"]);
            Assert.NotNull(service["ITem1"]);

            // make sure the history has the name it was initially created with instead of the name it was retrieved with
            // otherwise there might be problem with pushing changes back to the master repository                
            Assert.Equal("item1", service["ITem1"].Name);
        }

    }
}
