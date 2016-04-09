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


        #region Simple Tests (initial sync, no conflicts)


        [Fact]
        public void Synchronize_without_previous_sync_single_snapshots_and_no_conflicts_Addition()
        {
            //ARRANGE
            var left = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file1")
            };

            var right = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file2")
            };

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService.CreateHistory("history2");
            historyService["history1"].CreateSnapshot(left);
            historyService["history2"].CreateSnapshot(right);


            //ACT
            m_Instance.Synchronize(m_Group);


            //ASSERT
            var syncActionService = m_Group.GetSyncActionService();
            Assert.Equal(2, syncActionService.AllItems.Count());            

            SyncAssert.ActionsExist<AddFileSyncAction> (syncActionService, "/file1",
                expectedCount:1,
                expectedState: SyncActionState.Queued
                );

            SyncAssert.NewFileMacthes(left.GetFile("/file1"), syncActionService["/file1"].Single());            

            SyncAssert.ActionsExist<AddFileSyncAction>(syncActionService, "/file2",
                expectedCount:1,
                expectedState: SyncActionState.Queued);

            SyncAssert.NewFileMacthes(right.GetFile("/file2"), syncActionService["/file2"].Single());
            
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Single(m_Group.GetSyncPointService().Items);
        }

        [Fact]
        public void Synchronize_without_previous_sync_no_conflicts_Deletion()
        {
            // ARRANGE
            var left1 = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file1")
            };
            var left2 = new Directory(null, "root") 
            {
                // file 1 deleted
            };

            var right1 = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file2")
            };

            var right2 = new Directory(null, "root")
            {
                // file 2 deleted
            };

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService.CreateHistory("history2");
            historyService["history1"].CreateSnapshot(left1);
            historyService["history1"].CreateSnapshot(left2);
            historyService["history2"].CreateSnapshot(right1);
            historyService["history2"].CreateSnapshot(right2);


            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            var syncActions = m_Group.GetSyncActionService().AllItems.ToArray();
            Assert.Empty(syncActions);
            Assert.Single(m_Group.GetSyncPointService().Items);


        }


        

        #endregion

        //TODO: More tests

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

        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}