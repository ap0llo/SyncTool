using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Git.Common;
using SyncTool.Common.Groups;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.State
{
    /// <summary>
    /// Tests for <see cref="GitSyncPointService" />
    /// </summary>
    public class GitSyncPointServiceTest : GitGroupBasedTest
    {
        readonly IGroup m_Group;
        readonly GitSyncPointService m_Service;        

        public GitSyncPointServiceTest()
        {
            m_Group = CreateGroup();
            m_Service = m_Group.GetService<GitSyncPointService>();
        }


        [Fact]
        public void Items_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Service.Items);
        }

        [Fact]
        public void LatestSyncPoint_is_null_for_empty_repository()
        {
            Assert.Null(m_Service.LatestSyncPoint);
        }

        [Fact]
        public void AddItem_stores_the_state()
        {
            var state = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("value1");

            m_Service.AddItem(state);

            Assert.Single(m_Service.Items);
            Assert.True(m_Service.ItemExists(1));
            SyncAssert.Equal(state, m_Service[1]);
        }

        [Fact]
        public void AddItem_throws_ArgumentNullException_if_state_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service.AddItem(null));
        }

        [Fact]
        public void AddItem_throws_DuplicateSynchronizationStateException_if_state_id_already_exists()
        {
            var state1 = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("id");

            var state2 = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("id");

            m_Service.AddItem(state1);
            Assert.Throws<DuplicateSyncPointException>(() => m_Service.AddItem(state2));
        }

        [Fact]
        public void AddItem_correctly_stores_multiple_states()
        {
            var state1 = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("id");

            var state2 = SyncPointBuilder.NewSyncPoint()
                .WithId(2)
                .WithMultiFileSystemSnapshotId("id");

            m_Service.AddItem(state1);
            m_Service.AddItem(state2);

            Assert.Equal(2, m_Service.Items.Count());
            Assert.True(m_Service.ItemExists(1));
            Assert.True(m_Service.ItemExists(2));
        }

        [Fact]
        public void Values_from_disk_are_loaded_correctly()
        {
            var state1 = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("id1");

            var state2 = SyncPointBuilder.NewSyncPoint()
                .WithId(2)
                .WithMultiFileSystemSnapshotId("id3");
            
            m_Service.AddItem(state1);
            m_Service.AddItem(state2);

            // create another service instance that needs to load the state from disk

            var service  = new GitSyncPointService(m_Group.GetService<GitRepository>());

            Assert.Equal(2, service.Items.Count());
            Assert.True(service.ItemExists(1));
            Assert.True(service.ItemExists(2));
            
            SyncAssert.Equal(state1, service[1]);            
            SyncAssert.Equal(state2, service[2]);            
        }       

        [Fact]
        public void Indexer_throws_SynchronizationStateNotFoundException_for_unknown_state()
        {
            Assert.Throws<SyncPointNotFoundException>(() => m_Service[12]);
        }

        [Fact]
        public void Indexer_returns_expected_state()
        {
            var state = SyncPointBuilder.NewSyncPoint()
                .WithId(1)
                .WithMultiFileSystemSnapshotId("id");                

            m_Service.AddItem(state);
            SyncAssert.Equal(state, m_Service[1]);
        }

        [Fact]
        public void LatestSyncPoint_returns_the_expected_result()
        {
            {
                var state1 = SyncPointBuilder.NewSyncPoint()
                    .WithId(1)
                    .WithMultiFileSystemSnapshotId("id");

                m_Service.AddItem(state1);

                SyncAssert.Equal(state1, m_Service.LatestSyncPoint);
            }
            {
                var state2 = SyncPointBuilder.NewSyncPoint()
                    .WithId(2)
                    .WithMultiFileSystemSnapshotId("id");

                m_Service.AddItem(state2);

                SyncAssert.Equal(state2, m_Service.LatestSyncPoint);
            }
            
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}