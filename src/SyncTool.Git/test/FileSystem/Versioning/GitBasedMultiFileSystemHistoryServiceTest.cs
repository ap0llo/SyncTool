﻿using System;
using System.Linq;
using SyncTool.Common.Groups;
using SyncTool.FileSystem;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.FileSystem.Versioning;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization.TestHelpers;
using Xunit;

namespace SyncTool.Git.Test.FileSystem.Versioning
{
    public class GitBasedMultiFileSystemHistoryServiceTest : GitGroupBasedTest
    {
        readonly IGroup m_Group;
        readonly IHistoryService m_HistoryService;
        readonly GitBasedMultiFileSystemHistoryService m_Instance;

        public GitBasedMultiFileSystemHistoryServiceTest()
        {
            m_Group = CreateGroup();
            m_HistoryService = m_Group.GetHistoryService();
            m_Instance = m_Group.GetService<GitBasedMultiFileSystemHistoryService>();
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

            Assert.Equal(2, m_Instance.LatestSnapshot.HistoryNames.Count());
            Assert.True(m_Instance.LatestSnapshot.HistoryNames.Contains("history2"));
            Assert.True(m_Instance.LatestSnapshot.HistoryNames.Contains("history2"));
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


        [Fact]
        public void Indexer_throws_ArgumentNullException_if_snapshot_id_is_null_or_empty()
        {
            Assert.Throws<ArgumentNullException>(() => m_Instance[null]);
            Assert.Throws<ArgumentNullException>(() => m_Instance[""]);
            Assert.Throws<ArgumentNullException>(() => m_Instance[" "]);
            Assert.Throws<ArgumentNullException>(() => m_Instance["\t"]);
        }

        [Fact]
        public void Indexer_throws_SnapshotNotFoundException_for_unknown_snapshot_id()
        {
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance["SomeId"]);
        }

        [Fact]
        public void Indexer_returns_expected_snapshots()
        {
            // ARRANGE
            m_HistoryService.CreateHistory("history1");
            m_HistoryService.CreateHistory("history2");

            var expectedSnapshot = m_Instance.CreateSnapshot();

            //ACT
            var actualSnapshot = m_Instance[expectedSnapshot.Id];


            //ASSERT
            Assert.NotNull(actualSnapshot);
            Assert.Equal(expectedSnapshot.Id, actualSnapshot.Id);
            Assert.True(actualSnapshot.HistoryNames.SequenceEqual(expectedSnapshot.HistoryNames));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void GetChangedFiles_throws_ArgumentNullException(string invalidId)
        {
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChangedFiles(invalidId));
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChangedFiles("Irrelevant", invalidId));
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChangedFiles(invalidId, "Irrelevant"));
        }


        [Fact]
        public void GetChangedFiles_throws_SnapshotNotFoundException()
        {
            //  ARRANGE
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history1");
                historyBuilder.AddFile("file1");
                historyBuilder.CreateSnapshot();
            }
            var snapshot = m_Instance.CreateSnapshot();

            //ASSERT
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles("UnknownId"));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles("UnknownId", snapshot.Id));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChangedFiles(snapshot.Id, "UnknownId"));
        }

        [Fact]
        public void GetChangedFiles_returns_expected_result_1()
        {
            // ARRANGE
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history1");
                historyBuilder.AddFile("file1");
                historyBuilder.AddFile("file2");
                historyBuilder.CreateSnapshot();
            }
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history2");
                historyBuilder.AddFile("file1");                
                historyBuilder.CreateSnapshot();
            }

            var snapshot = m_Instance.CreateSnapshot();

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot.Id);

            //ASSERT
            var expected = new[] {"/file1", "/file2"};
            Assert.Empty(changedFiles.Except(expected));
            Assert.Empty(expected.Except(changedFiles));
        }

        [Fact]
        public void GetChangedFiles_returns_expected_result_2()
        {
            // ARRANGE
            
            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.AddFile("file2");
            historyBuilder1.CreateSnapshot();
                        
            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file1");
            historyBuilder2.CreateSnapshot();
            
            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder1.AddFile("file3");
            historyBuilder1.CreateSnapshot();

            historyBuilder2.AddFile("file4");
            historyBuilder2.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            var expected = new[] { "/file3", "/file4" };
            Assert.Empty(changedFiles.Except(expected));
            Assert.Empty(expected.Except(changedFiles));
        }

        [Fact]
        public void GetChangedFiles_returns_expected_result_3()
        {
            // ARRANGE
            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.AddFile("file2");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file1");
            historyBuilder2.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder1.AddFile("file3");
            historyBuilder1.CreateSnapshot();
            
            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            var expected = new[] { "/file3" };
            Assert.Empty(changedFiles.Except(expected));
            Assert.Empty(expected.Except(changedFiles));
        }

        [Fact]
        public void GetChangedFiles_can_handle_histories_added_between_snapshots()
        {
            // ARRANGE
            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file2");
            historyBuilder2.CreateSnapshot();
            
            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var changedFiles = m_Instance.GetChangedFiles(snapshot1.Id, snapshot2.Id);

            //ASSERT
            var expected = new[] { "/file2" };
            Assert.Empty(changedFiles.Except(expected));
            Assert.Empty(expected.Except(changedFiles));
        }

        [Fact]
        public void GetChangedFiles_throws_InvalidRangeException()
        {
            // ARRANGE
            var historyBuilder = new HistoryBuilder(m_Group, "history1");
            historyBuilder.AddFile("file1");
            historyBuilder.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder.AddFile("file2");
            historyBuilder.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT & ASSERT
            Assert.Throws<InvalidRangeException>(() => m_Instance.GetChangedFiles(snapshot2.Id, snapshot1.Id));
        }

        //TODO: GetChangedFiles_can_handle_histories_removed_between_snapshots

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        public void GetChanges_throws_ArgumentNullException(string invalidId)
        {
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChanges(invalidId));
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChanges("Irrelevant", invalidId));
            Assert.Throws<ArgumentNullException>(() => m_Instance.GetChanges(invalidId, "Irrelevant"));
        }

        [Fact]
        public void GetChanges_throws_SnapshotNotFoundException()
        {
            //  ARRANGE
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history1");
                historyBuilder.AddFile("file1");
                historyBuilder.CreateSnapshot();
            }
            var snapshot = m_Instance.CreateSnapshot();

            //ASSERT
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("UnknownId"));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges("UnknownId", snapshot.Id));
            Assert.Throws<SnapshotNotFoundException>(() => m_Instance.GetChanges(snapshot.Id, "UnknownId"));
        }

        [Fact]
        public void GetChanges_returns_expected_result_1()
        {
            // ARRANGE
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history1");
                historyBuilder.AddFile("file1");
                historyBuilder.AddFile("file2");
                historyBuilder.CreateSnapshot();
            }
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history2");
                historyBuilder.AddFile("file3");
                historyBuilder.CreateSnapshot();
            }

            var snapshot = m_Instance.CreateSnapshot();

            //ACT
            var diff = m_Instance.GetChanges(snapshot.Id);
            
            //ASSERT
            Assert.Null(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);
  
            Assert.NotNull(diff.FileChanges);
            Assert.Equal(3, diff.FileChanges.Count());
            Assert.True(diff.FileChanges.All(cl => cl.AllChanges.Count() == 1));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file1").GetChanges("history1"));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file2").GetChanges("history1"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history1"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file1").GetChanges("history2"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file2").GetChanges("history2"));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history2"));

            Assert.NotNull(diff.HistoryChanges);
            Assert.Equal(2, diff.HistoryChanges.Count());
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history1" , ChangeType.Added))));
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history2" , ChangeType.Added))));            
        }

        [Fact]
        public void GetChanges_returns_expected_result_2()
        {
            // ARRANGE

            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.AddFile("file2");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file1");
            historyBuilder2.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder1.AddFile("file3");
            historyBuilder1.CreateSnapshot();

            historyBuilder2.AddFile("file4");
            historyBuilder2.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.NotNull(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);

            Assert.NotNull(diff.FileChanges);
            Assert.Equal(2, diff.FileChanges.Count());
            Assert.True(diff.FileChanges.All(cl => cl.AllChanges.Count() == 1));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history1"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file4").GetChanges("history1"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history2"));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file4").GetChanges("history2"));

            Assert.NotNull(diff.HistoryChanges);
            Assert.Equal(2, diff.HistoryChanges.Count());
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history1", ChangeType.Modified))));
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history2", ChangeType.Modified))));

        }

        [Fact]
        public void GetChanges_returns_expected_result_3()
        {
            // ARRANGE
            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.AddFile("file2");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file1");
            historyBuilder2.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder1.AddFile("file3");
            historyBuilder1.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT
            Assert.NotNull(diff.FromSnapshot);
            Assert.NotNull(diff.ToSnapshot);

            Assert.NotNull(diff.FileChanges);
            Assert.Single(diff.FileChanges);
            Assert.Single(diff.FileChanges.Single().AllChanges);
            Assert.Equal("/file3", diff.FileChanges.Single().Path);
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history1"));
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file3").GetChanges("history2"));

            Assert.NotNull(diff.HistoryChanges);
            Assert.Single(diff.HistoryChanges);
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history1", ChangeType.Modified))));            
        }

        [Fact]
        public void GetChanges_can_handle_histories_added_between_snapshots()
        {
            // ARRANGE
            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
            historyBuilder2.AddFile("file2");
            historyBuilder2.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT
            var diff = m_Instance.GetChanges(snapshot1.Id, snapshot2.Id);

            //ASSERT            
            Assert.NotNull(diff.FileChanges);
            Assert.Single(diff.FileChanges);
            Assert.Single(diff.FileChanges.Single().AllChanges);
            Assert.Equal("/file2", diff.FileChanges.Single().Path);
            Assert.Empty(diff.FileChanges.Single(cl => cl.Path == "/file2").GetChanges("history1"));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file2").GetChanges("history2"));


            Assert.NotNull(diff.HistoryChanges);
            Assert.Single(diff.HistoryChanges);
            Assert.Single(diff.HistoryChanges.Where(c => c.Equals(new HistoryChange("history2", ChangeType.Added))));
        }
        [Fact]
        public void GetChanges_combines_identical_changes_from_different_histories()
        {  
            // ARRANGE
            var lastWriteTime = DateTime.Now;
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history1");
                historyBuilder.AddFile("file1", lastWriteTime);                
                historyBuilder.CreateSnapshot();
            }
            {
                var historyBuilder = new HistoryBuilder(m_Group, "history2");
                historyBuilder.AddFile("file1", lastWriteTime);
                historyBuilder.CreateSnapshot();
            }

            var snapshot = m_Instance.CreateSnapshot();

            //ACT
            var diff = m_Instance.GetChanges(snapshot.Id);

            //ASSERT
            
            Assert.NotNull(diff.FileChanges);
            Assert.Single(diff.FileChanges);
            Assert.Single(diff.FileChanges.Single().AllChanges);
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file1").GetChanges("history1"));
            Assert.Single(diff.FileChanges.Single(cl => cl.Path == "/file1").GetChanges("history2"));
        }


        [Fact]
        public void GetChanges_throws_InvalidRangeException()
        {
            // ARRANGE
            var historyBuilder = new HistoryBuilder(m_Group, "history1");
            historyBuilder.AddFile("file1");
            historyBuilder.CreateSnapshot();

            var snapshot1 = m_Instance.CreateSnapshot();

            historyBuilder.AddFile("file2");
            historyBuilder.CreateSnapshot();

            var snapshot2 = m_Instance.CreateSnapshot();

            //ACT & ASSERT
            Assert.Throws<InvalidRangeException>(() => m_Instance.GetChanges(snapshot2.Id, snapshot1.Id));
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}