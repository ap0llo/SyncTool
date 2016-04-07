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
        public void Synchronize_without_previous_sync_single_snapshots_and_no_conflicts()
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

            Assert.Single(syncActionService["/file1"]);
            Assert.Equal(SyncActionState.Queued, syncActionService["/file1"].Single().State);
            Assert.IsType<AddFileSyncAction>(syncActionService["/file1"].Single());
            Assert.True(((AddFileSyncAction)syncActionService["/file1"].Single()).NewFile.Matches(left.GetFile("file1")));

            Assert.Single(syncActionService["/file2"]);
            Assert.Equal(SyncActionState.Queued, syncActionService["/file2"].Single().State);           
            Assert.IsType<AddFileSyncAction>(syncActionService["/file2"].Single());
            Assert.True(((AddFileSyncAction)syncActionService["/file2"].Single()).NewFile.Matches(right.GetFile("file2")));

            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Single(m_Group.GetSyncPointService().Items);

        }

        #endregion

        //TODO: More tests


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}