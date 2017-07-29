using SyncTool.Common.Services;
using SyncTool.Configuration;
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
    public class SqlConfigurationServiceTest : SqlTestBase
    {      

        #region Items

        [Fact]
        public void Items_is_empty_for_empty_database()
        {            
            var instance = new SqlConfigurationService(m_Context);
            Assert.Empty(instance.Items);
        }

        #endregion


        #region AddItem

        [Fact]
        public void AddItem_stores_the_item_in_the_database()
        {
            var syncFolder = new SyncFolder("folder1") { Path = "foo" };

            var service = new SqlConfigurationService(m_Context);
            service.AddItem(syncFolder);

            Assert.Single(service.Items);
            Assert.Equal(syncFolder, service.Items.Single());

        }

        [Fact]
        public void AddItem_throws_DuplicateSyncFolderException()
        {
            var syncFolder1 = new SyncFolder("folder1") { Path = "foo" };
            var syncFolder2 = new SyncFolder("folder1") { Path = "bar" };

            var service= new SqlConfigurationService(m_Context);
            service.AddItem(syncFolder1);
            Assert.Throws<DuplicateSyncFolderException>(() => service.AddItem(syncFolder2));
        }

        [Fact]
        public void AddItem_throws_DuplicateSyncFolderException_if_item_with_different_casing_already_exists()
        {
            var syncFolder1 = new SyncFolder("folder1") { Path = "foo" };
            var syncFolder2 = new SyncFolder("folDER1") { Path = "bar" };

            var service = new SqlConfigurationService(m_Context);
            service.AddItem(syncFolder1);
            Assert.Throws<DuplicateSyncFolderException>(() => service.AddItem(syncFolder2));
        }

        #endregion


        #region UpdateItem

        [Fact]
        public void UpdateItem_throws_SyncFolderNotFoundException_if_folder_does_not_exist()
        {
            var updatedFolder = new SyncFolder("NewFolder");

            var service = new SqlConfigurationService(m_Context);
            Assert.Throws<SyncFolderNotFoundException>(() => service.UpdateItem(updatedFolder));
        }

        [Fact]
        public void UpdateItem_stores_the_updated_item_in_the_database()
        {
            var folder = new SyncFolder("SyncFolder") { Path = "Path" };

            var service = new SqlConfigurationService(m_Context);
            service.AddItem(folder);
           
            var service2 = new SqlConfigurationService(m_Context);
            Assert.Equal(folder.Path, service2["SyncFolder"].Path);

        }

        
        #endregion


        #region Indexer

        [Fact]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            var service = new SqlConfigurationService(m_Context);
            Assert.Throws<ArgumentNullException>(() => service[null]);
            Assert.Throws<ArgumentNullException>(() => service[""]);
            Assert.Throws<ArgumentNullException>(() => service[" "]);
        }

        [Fact]
        public void Indexer_Get_throws_ItemNotFoundException_if_the_requested_item_was_not_found()
        {
            var service = new SqlConfigurationService(m_Context);
            Assert.Throws<ItemNotFoundException>(() => service["SomeName"]);
        }

        [Fact]
        public void Indexer_Get_returns_the_expected_Item()
        {
            var service = new SqlConfigurationService(m_Context);
            service.AddItem(new SyncFolder("folder1"));
            Assert.NotNull(service["folder1"]);
            // name has to be treated case-invariant
            Assert.NotNull(service["foLDEr1"]);
        }

        #endregion


        #region ItemExists

        [Fact]
        public void ItemExists_compares_names_case_invariant()
        {
            var service = new SqlConfigurationService(m_Context);
            service.AddItem(new SyncFolder("folder1"));

            Assert.True(service.ItemExists("folder1"));
            Assert.True(service.ItemExists("foLDEr1"));
        }

        #endregion

    }
}
