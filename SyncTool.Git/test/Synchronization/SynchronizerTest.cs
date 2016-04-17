// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization
{
    /// <summary>
    /// Tests for <see cref="Synchronizer"/>.
    /// Tests are located in SyncTool.Git so git implementations of the services can be used instead of mocking everything
    /// </summary>
    public class SynchronizerTest : GitGroupBasedTest
    {
        readonly Synchronizer m_Instance;
        readonly IGroup m_Group;

        public SynchronizerTest()
        {
            m_Instance = new Synchronizer(EqualityComparer<IFileReference>.Default);
            m_Group = CreateGroup();
        }

        [Fact]
        public void Running_Synchronize_without_histories_has_no_effect()
        {            
            m_Instance.Synchronize(m_Group);

            // check that no actions / conflicts or sync points have been stored

            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);

        }

        [Fact]
        public void Running_Synchronize_without_at_two_histories_has_no_effect()
        {
            // ARRANGE
            var state = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file1")
            };

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService["history1"].CreateSnapshot(state);


            // ACT
            m_Instance.Synchronize(m_Group);


            // ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);
        }

        [Fact]
        public void Running_Synchronize_without_at_least_one_snapshot_per_history_has_no_effect()
        {
            //ARRANGE
            var state = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file1")
            };
            
            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService.CreateHistory("history2");
            historyService["history1"].CreateSnapshot(state);

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);

            
        }

        [Fact]
        public void Running_Sychronize_without_any_changes_since_the_last_sync_point_produces_no_actions_or_conflicts()
        {
            //ARRANGE
            var state1 = new Directory(null, "root") { d => new EmptyFile(d, "file1") };
            var state2 = new Directory(null, "root") { d => new EmptyFile(d, "file2")};

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService.CreateHistory("history2");
            var snapshot1 = historyService["history1"].CreateSnapshot(state1);
            var snapshot2 = historyService["history2"].CreateSnapshot(state2);

            // save a sync point
            var syncPoint = new MutableSyncPoint()
            {
                Id = 1,
                FromSnapshots = null,
                ToSnapshots = new Dictionary<string, string>()
                {
                    {"history1", snapshot1.Id},
                    {"history2", snapshot2.Id}
                }
            };

            m_Group.GetSyncPointService().AddItem(syncPoint);

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Equal(2, m_Group.GetSyncPointService().Items.Count());
        }
 
        [Fact]
        public void Synchronize_without_previous_sync_single_snapshots_and_no_conflicts()
        {
            //ARRANGE
            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            left.AddFile("file1");
            right.AddFile("file2");
                     
            var snapshot1 = left.CreateSnapshot();
            var snapshot2 = right.CreateSnapshot();

            //ACT
            m_Instance.Synchronize(m_Group);


            //ASSERT
            var syncActionService = m_Group.GetSyncActionService();
            Assert.Equal(2, syncActionService.AllItems.Count());            

            SyncAssert.ActionsExist<AddFileSyncAction> (syncActionService, "/file1",
                expectedCount:1,
                expectedState: SyncActionState.Queued
                );

            SyncAssert.NewFileMacthes(left.CurrentState.GetFile("/file1"), syncActionService["/file1"].Single());            

            SyncAssert.ActionsExist<AddFileSyncAction>(syncActionService, "/file2",
                expectedCount:1,
                expectedState: SyncActionState.Queued);

            SyncAssert.NewFileMacthes(right.CurrentState.GetFile("/file2"), syncActionService["/file2"].Single());
            
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Single(m_Group.GetSyncPointService().Items);

            var syncPoint = m_Group.GetSyncPointService().Items.Single();
            Assert.Equal(1, syncPoint.Id);
            Assert.Null(syncPoint.FromSnapshots);
            var expectedToSnapshots = new Dictionary<string, string>()
            {
                { "left", snapshot1.Id },
                { "right", snapshot2.Id }
            };
            DictionaryAssert.Equal(expectedToSnapshots, syncPoint.ToSnapshots);
        }

        [Fact]
        public void Synchronize_without_previous_sync_no_conflicts()
        {
            // ARRANGE
            var left = new HistoryBuilder(m_Group, "left");
            left.AddFile("file1");
            left.CreateSnapshot();

            left.RemoveFile("file1");
            left.CreateSnapshot();

            var right = new HistoryBuilder(m_Group, "right");

            right.AddFile("file2");
            right.CreateSnapshot();

            right.RemoveFile("file2");
            right.CreateSnapshot();

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            var syncActions = m_Group.GetSyncActionService().AllItems.ToArray();
            Assert.Empty(syncActions);
            Assert.Single(m_Group.GetSyncPointService().Items);
        }
        
        [Fact]
        public void Synchronize_Cycle_with_conflict()
        {
            //ARRANGE
            // changes to file1 (captial letters indicate file versions)
            // Left: A -> B -> C -> A
            // Right: B -> D

            var lastWriteTime = DateTime.Now;
            var a =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };
            var d =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(4) } };

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["left"].CreateSnapshot(b);
            historyService["left"].CreateSnapshot(c);
            historyService["left"].CreateSnapshot(a);

            historyService["right"].CreateSnapshot(b);
            historyService["right"].CreateSnapshot(d);

            //ACT
            m_Instance.Synchronize(m_Group);

            // ASSERT
            // Expected result: conflict between file versions A and D, no sync actions            
            Assert.Single(m_Group.GetSyncConflictService().Items);            
            //TODO: Check that the right snapshot Ids were saved for the conflict
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Single(m_Group.GetSyncPointService().Items);

        }

        [Fact]
        public void Synchronize_Cycle_without_conflict()
        {
            //ARRANGE
            // changes to file (captial letters indicate file versions)
            // Left: A -> B -> A
            // Right: C -> A

            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };            

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["left"].CreateSnapshot(b);
            historyService["left"].CreateSnapshot(a);

            historyService["right"].CreateSnapshot(c);
            historyService["right"].CreateSnapshot(a);


            //ACT
            m_Instance.Synchronize(m_Group);

            // ASSERT            
            var items = m_Group.GetSyncConflictService().Items.ToArray();
            Assert.Empty(items);            
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Single(m_Group.GetSyncPointService().Items);

        }

        [Fact]
        public void Synchronize_with_previous_sync_point_01()
        {
            // ARRANGE      
            var historyService = m_Group.GetHistoryService();

            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            // left: add file 1
            // right: empty
            left.AddFile("file1");
            left.CreateSnapshot();
            right.AddFiles();
            right.CreateSnapshot();

            m_Instance.Synchronize(m_Group);
            Assert.Equal(1, m_Group.GetSyncActionService().AllItems.Count());

            // left: add file 2
            left.AddFiles("file2");
            left.CreateSnapshot();         

            //ACT
            m_Instance.Synchronize(m_Group);

            Assert.Equal(2, m_Group.GetSyncPointService().Items.Count());
            Assert.Equal(2, m_Group.GetSyncActionService().AllItems.Count());
            //TODO: Check ids in syncpoints
        }


        [Fact]
        public void Synchronize_non_applicable_sync_actions_yields_conflict_and_get_cancelled()
        {
            //ARRANGE
            // Left: A -> Sync ------> Sync -> Conflict
            // Right:     Sync -> B -> Sync -> Conflict
            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            
            
            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["right"].CreateSnapshot(new Directory(null, "root"));

            // ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file", 
                expectedState: SyncActionState.Queued, 
                expectedCount: 1);
            Assert.Empty(m_Group.GetSyncConflictService().Items);

            historyService["right"].CreateSnapshot(b);

            // second sync
            
            m_Instance.Synchronize(m_Group);

            // ASSERT

            // the sync action needs to be cancelled, a conflcit should exist
            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Cancelled,
                expectedCount: 1);

            Assert.Single(m_Group.GetSyncConflictService().Items);
            var conflict = m_Group.GetSyncConflictService().Items.Single();
            Assert.Equal("/file", conflict.FilePath);
            Assert.Null(conflict.SnapshotIds);
        }


        [Fact]
        public void Synchronize_pending_actions_are_cancelled_if_a_conflict_is_detected()
        {
            //ARRANGE
            // 1: A -> Sync -> B -> Sync
            // 2: A -> Sync -> C -> Sync
            // 3: * -> Sync -> ---> Sync
            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };


            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("1");
            historyService.CreateHistory("2");
            historyService.CreateHistory("3");

           var snapshot1 = historyService["1"].CreateSnapshot(a);
           var snapshot2 = historyService["2"].CreateSnapshot(a);
           var snapshot3 = historyService["3"].CreateSnapshot(new Directory(null, "root"));

            // ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Queued,
                expectedCount: 1);
            Assert.Empty(m_Group.GetSyncConflictService().Items);


            historyService["1"].CreateSnapshot(b);
            historyService["2"].CreateSnapshot(c);
            

            // second sync

            m_Instance.Synchronize(m_Group);

            // ASSERT

            // the sync action needs to be cancelled, a conflcit should exist
            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Cancelled,
                expectedCount: 1);

            Assert.Single(m_Group.GetSyncConflictService().Items);
            var conflict = m_Group.GetSyncConflictService().Items.Single();
            Assert.Equal("/file", conflict.FilePath);            
            Assert.Null(conflict.SnapshotIds);
        }


        [Fact]
        public void Synchronize_Unrelated_sync_actions_stay_unchanged()
        {
            // ARRANGE      
            var historyService = m_Group.GetHistoryService();

            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            left.AddFile("file1");

            left.CreateSnapshot();
            right.CreateSnapshot();


            //ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file1",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1);

            Assert.Empty(m_Group.GetSyncConflictService().Items);

            left.AddFile("file2");
            left.CreateSnapshot();

            m_Instance.Synchronize(m_Group);


            //ASSERT
            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file1",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1);

            SyncAssert.ActionsExist<AddFileSyncAction>(m_Group.GetSyncActionService(), "/file2",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1);

            Assert.Empty(m_Group.GetSyncConflictService().Items);
        }


        //unrelated sync actions stay unchanged

        //TODO: More tests


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }





        class HistoryBuilder
        {
            readonly IGroup m_Group;
            readonly string m_Name;
            IDictionary<string, File> m_Files = new Dictionary<string, File>(StringComparer.InvariantCultureIgnoreCase);


            public IDirectory CurrentState { get; private set; }

            public HistoryBuilder(IGroup group, string name)
            {
                m_Group = @group;
                m_Name = name;
                m_Group.GetHistoryService().CreateHistory(m_Name);
            }

            
            public void AddFile(string file) => AddFiles(file);

            public void AddFiles(params string[] files)
            {
                foreach (var fileName in files)
                {
                    if (!m_Files.ContainsKey(fileName))
                    {
                        m_Files.Add(fileName, new File(null, fileName) { LastWriteTime = DateTime.Now });
                    }
                }
                UpdateCurrentState();
            }

            public void RemoveFile(string file) => RemoveFiles(file);

            public void RemoveFiles(params string[] files)
            {
                foreach (var fileName in files)
                {
                    m_Files.Remove(fileName);
                }
                UpdateCurrentState();
            }

            public void SetLastWriteTime(string fileName, DateTime lastWriteTime)
            {
                m_Files[fileName].LastWriteTime = lastWriteTime;
                UpdateCurrentState();
            }


            public IFileSystemSnapshot CreateSnapshot()
            {
                UpdateCurrentState();
                return m_Group.GetHistoryService()[m_Name].CreateSnapshot(CurrentState);
            }


            void UpdateCurrentState()
            {
                var dir = new Directory(null, "root");
                foreach (var file in m_Files.Values)
                {
                    dir.Add(d => file.WithParent(d));
                }
                CurrentState = dir;
            }
        }

    }
}