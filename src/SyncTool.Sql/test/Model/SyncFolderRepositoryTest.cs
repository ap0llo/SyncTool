using SyncTool.Sql.Model;
using SyncTool.Sql.TestHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SyncTool.Sql.Test.Model
{
    public class SyncFolderRepositoryTest : SqlTestBase
    {       
        [Fact]
        public void UpdateItem_throws_DatabaseUpdateException_if_syncfolder_version_does_not_match()
        {
            var instance = new SyncFolderRepository(ContextFactory);

            var syncFolder = new SyncFolderDo() { Name = "folder1" };
            var version1 = instance.AddItem(syncFolder);
            instance.UpdateItem(version1);

            var version2 = instance.GetItemOrDefault("folder1");
            var version3 = instance.GetItemOrDefault("folder1");

            version2.Path = "Path v2";
            instance.UpdateItem(version2);

            version3.Path = "Path v3";
            Assert.Throws<DatabaseUpdateException>(() => instance.UpdateItem(version3));

            Assert.Equal(version2.Path, instance.GetItemOrDefault("folder1").Path);
            Assert.Equal(2, version2.Version);
        }


    }
}
