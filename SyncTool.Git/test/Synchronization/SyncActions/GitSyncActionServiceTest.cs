// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization;
using SyncTool.Synchronization.SyncActions;
using Xunit;

namespace SyncTool.Git.Synchronization.SyncActions
{
    /// <summary>
    /// Tests for <see cref="GitSyncActionService"/>
    /// </summary>
    public class GitSyncActionServiceTest : GitGroupBasedTest
    {

        readonly GitBasedGroup m_Group;
        readonly GitSyncActionService m_Service;

        public GitSyncActionServiceTest()
        {
            m_Group = CreateGroup();
            m_Service = new GitSyncActionService(m_Group);
        }


        #region AllItems

        [Fact]
        public void AllItems_returns_empty_enumerable_if_branch_does_not_exits()
        {
            Assert.Empty(m_Service.AllItems);
        }
        

        [Fact]
        public void AllItems_returns_empty_enumerable_if_branch_is_empty()
        {
            m_Group.Repository.CreateBranch(GitSyncActionService.BranchName, m_Group.Repository.GetInitialCommit());
            Assert.Empty(m_Service.AllItems);
        }


        [Fact]
        public void AllItems_returns_expected_result()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 42, new FileReference("/path/to/file1"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target2", SyncActionState.Cancelled, 42, new FileReference("/path/to/file2"));
            var action3 = new AddFileSyncAction(Guid.NewGuid(), "target3", SyncActionState.Active, 42, new FileReference("/path/to/file3"));
            
            m_Service.AddItems(action1, action2, action3);

            var allItems = m_Service.AllItems.ToList();
            Assert.Equal(3, allItems.Count());

            Assert.Single(allItems.Where(a => a.State == SyncActionState.Queued));            
            Assert.Single(allItems.Where(a => a.State == SyncActionState.Cancelled));
            Assert.Single(allItems.Where(a => a.State == SyncActionState.Active));

        }

        #endregion

        #region Indexer

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer validates the file path")]
        public void Indexer_validates_the_file_path()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service[SyncActionState.Active, null]);
            Assert.Throws<FormatException>(() => m_Service[SyncActionState.Active, " "]);
            Assert.Throws<FormatException>(() => m_Service[SyncActionState.Active, "\\"]);
            Assert.Throws<FormatException>(() => m_Service[SyncActionState.Active, "/"]);
            Assert.Throws<FormatException>(() => m_Service[SyncActionState.Active, "fileName"]);
            Assert.Throws<FormatException>(() => m_Service[SyncActionState.Active, "relative/path/to/file"]);


            Assert.Throws<ArgumentNullException>(() => m_Service[null]);
            Assert.Throws<FormatException>(() => m_Service[" "]);
            Assert.Throws<FormatException>(() => m_Service["\\"]);
            Assert.Throws<FormatException>(() => m_Service["/"]);
            Assert.Throws<FormatException>(() => m_Service["fileName"]);
            Assert.Throws<FormatException>(() => m_Service["relative/path/to/file"]);
        }
        
        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if branch does not exist")]
        public void Indexer_returns_empty_enumerable_if_branch_does_not_exits()
        {            
            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/somePath"]);

            Assert.Empty(m_Service["/somePath"]);            
        }
        
        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if branch is empty")]
        public void Indexer_returns_empty_enumerable_if_branch_is_empty()
        {
            m_Group.Repository.CreateBranch(GitSyncActionService.BranchName, m_Group.Repository.GetInitialCommit());

            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/somePath"]);

            Assert.Empty(m_Service["/somePath"]);
        }


        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if no actions in the specified state exist")]
        public void Indexer_returns_empty_enumerable_if_no_actions_in_the_specified_state_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 23, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if no actions for the specified path exist")]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_path_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 23, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[action.FilePath]);
            Assert.Empty(m_Service["/some/path"]);            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if no actions for the specified state and path exist")]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_state_and_path_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 42, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued, action.FilePath]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/some/path"]);            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns all actions for the specified state")]
        public void Indexer_returns_all_actions_for_the_specified_state()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 42, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 42, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Equal(2, m_Service[SyncActionState.Queued].Count());            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + "Indexer returns all actions for the specified path")]
        public void Indexer_returns_all_actions_for_the_specified_path()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued,42,  new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, 42, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Single(m_Service[action1.FilePath]);
            Assert.Single(m_Service[action2.FilePath]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + "Indexer returns all actions for the specified state and path")]
        public void Indexer_returns_all_actions_for_the_specified_state_and_path()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued,23, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued,23, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Single(m_Service[SyncActionState.Queued, action1.FilePath]);
            Assert.Single(m_Service[SyncActionState.Queued, action2.FilePath]);
        }

        #endregion


        #region AddItems

        [Fact(DisplayName = nameof(GitSyncActionService) + ".AddItems() stores new actions")]
        public void AddItems_stores_new_actions()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, 23,new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target2", SyncActionState.Active, 23, new FileReference("/path/to/some/Other/file"));

            m_Service.AddItems(action1, action2);

            var newService = new GitSyncActionService(m_Group);

            Assert.Equal(2, newService[SyncActionState.Active].Count());
            Assert.Single(newService[SyncActionState.Active, action1.FilePath]);
            Assert.Single(newService[SyncActionState.Active, action2.FilePath]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".AddItems() throws " + nameof(DuplicateSyncActionException) + " if action already exists")]
        public void AddItems_throws_DuplicateSyncActionException_if_action_already_exists()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target", SyncActionState.Active, 23, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(action1.Id, "target", SyncActionState.Active, 42, new FileReference("/path/to/file"));

            m_Service.AddItems(action1);
            Assert.Throws<DuplicateSyncActionException>(() => m_Service.AddItems(action2));
        }

        #endregion


        #region UpdateItems

        [Fact(DisplayName = nameof(GitSyncActionService) + ".UpdateItems() updates actions")]
        public void UpdateItems_updates_actions()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued,42, new FileReference("/path/to/file"));
            var updatedAction = new AddFileSyncAction(action.Id, "target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            
            m_Service.AddItems(action);
            Assert.Single(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);

            m_Service.UpdateItems(updatedAction);
            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Single(m_Service[SyncActionState.Active]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".UpdateItems() throws " + nameof(SyncActionNotFoundException) + " if action does not exist")]
        public void UpdateItems_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var updatedAction = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.UpdateItems(updatedAction));
        }

        #endregion


        #region RemoveItems

        [Fact(DisplayName = nameof(GitSyncActionService) + ".RemoveItems() throws " + nameof(SyncActionNotFoundException) + " if action does not exist")]
        public void RemoveItems_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var removedAction = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.RemoveItems(removedAction));
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".RemoveItems() throws " + nameof(SyncActionNotFoundException) + " if action with the same state does not exist")]
        public void RemoveItems_throws_SyncActionNotFoundException_if_action_with_the_same_state_does_not_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            var removedAction = new AddFileSyncAction(action.Id, "target1", SyncActionState.Queued, 42,new FileReference("/path/to/file"));

            Assert.Throws<SyncActionNotFoundException>(() => m_Service.RemoveItems(removedAction));
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".RemoveItems() removes an action")]
        public void RemoveItems_removes_an_action()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, 42, new FileReference("/path/to/file"));

            m_Service.AddItems(action);
            Assert.Single(m_Service[action.State]);

            m_Service.RemoveItems(action);
            Assert.Empty(m_Service[action.State]);
        }

        #endregion
        

        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}