// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.TestHelpers;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryServiceTest : GitGroupBasedTest
    {
        readonly GitBasedGroup m_Group;
        readonly IHistoryService m_HistoryService;
        readonly GitBasedMultiFileSystemHistoryService m_Instance;

        public GitBasedMultiFileSystemHistoryServiceTest()
        {
            m_Group = CreateGroup();
            m_HistoryService = m_Group.GetHistoryService();

            m_Instance = new GitBasedMultiFileSystemHistoryService(m_Group, m_HistoryService);
        }



        [Fact]
        public void LatestSnapshot_is_null_for_empty_repository()
        {
            Assert.Null(m_Instance.LatestSnapshot);
        }


        [Fact]
        public void Snapshots_is_empty_for_empty_repository()
        {
            Assert.NotNull(m_Instance.Snapshots);
            Assert.Empty(m_Instance.Snapshots);
        }


        [Fact]
        public void CreateSnapshot_does_not_create_a_snapshot_if_no_histories_exist()
        {
            m_Instance.CreateSnapshot();
            
            Assert.Empty(m_Instance.Snapshots);
        }

        [Fact]
        public void CreateSnapshot_creates_a_snapshot_if_empty_histories_exist()
        {
            // ARRANGE
            m_HistoryService.CreateHistory("history1");
            m_HistoryService.CreateHistory("history2");

            // ACT
            m_Instance.CreateSnapshot();

            //ASSERT
            Assert.NotNull(m_Instance.LatestSnapshot);
            Assert.Single(m_Instance.Snapshots);

            Assert.Equal(2, m_Instance.LatestSnapshot.HistoyNames.Count());
            Assert.True(m_Instance.LatestSnapshot.HistoyNames.Contains("history2"));
            Assert.True(m_Instance.LatestSnapshot.HistoyNames.Contains("history2"));
        }


        [Fact]
        public void Getting_a_filesystem_snapshot_from_a_snapshot_created_for_an_empty_history_returns_null()
        {
            //ARRANGE
            // create history, but do not create a filesystem snapshot
            m_HistoryService.CreateHistory("history1");

            //ACT
            m_Instance.CreateSnapshot();
        
            //ASSERT
            var snapshot = m_Instance.LatestSnapshot;
            Assert.Null(snapshot.GetSnapshot("history1"));
        }

        [Fact]
        public void Getting_a_filesystem_snapshot_from_a_snapshot_returns_the_expected_snapshot()
        {
            //ARRANGE
            // create history, but do not create a filesystem snapshot
            m_HistoryService.CreateHistory("history1");

            var createdSnapshot = m_HistoryService["history1"].CreateSnapshot(new Directory(null, "dir1"));

            //ACT
            m_Instance.CreateSnapshot();

            //ASSERT
            var fileSystemSnapshot = m_Instance.LatestSnapshot.GetSnapshot("history1");
            Assert.NotNull(fileSystemSnapshot);
            Assert.Equal(createdSnapshot.Id, fileSystemSnapshot.Id);
            Assert.Equal(createdSnapshot.CreationTime, fileSystemSnapshot.CreationTime);
            FileSystemAssert.DirectoryEqual(createdSnapshot.RootDirectory, fileSystemSnapshot.RootDirectory);
        }

        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}