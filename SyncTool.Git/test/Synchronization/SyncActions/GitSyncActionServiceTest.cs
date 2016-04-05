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
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));

            m_Service.Add(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if no actions for the specified path exist")]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_path_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));

            m_Service.Add(action);

            Assert.NotEmpty(m_Service[action.FilePath]);
            Assert.Empty(m_Service["/some/path"]);            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns empty enumerable if no actions for the specified state and path exist")]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_state_and_path_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));

            m_Service.Add(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued, action.FilePath]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/some/path"]);            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + " Indexer returns all actions for the specified state")]
        public void Indexer_returns_all_actions_for_the_specified_state()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/another/file"));

            m_Service.Add(action1, action2);

            Assert.Equal(2, m_Service[SyncActionState.Queued].Count());            
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + "Indexer returns all actions for the specified path")]
        public void Indexer_returns_all_actions_for_the_specified_path()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/another/file"));

            m_Service.Add(action1, action2);

            Assert.Single(m_Service[action1.FilePath]);
            Assert.Single(m_Service[action2.FilePath]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + "Indexer returns all actions for the specified state and path")]
        public void Indexer_returns_all_actions_for_the_specified_state_and_path()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/another/file"));

            m_Service.Add(action1, action2);

            Assert.Single(m_Service[SyncActionState.Queued, action1.FilePath]);
            Assert.Single(m_Service[SyncActionState.Queued, action2.FilePath]);
        }

        #endregion


        #region Add

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Add() stores new actions")]
        public void Add_stores_new_actions()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(Guid.NewGuid(), "target2", SyncActionState.Active, new FileReference("/path/to/some/Other/file"));

            m_Service.Add(new[] {action1, action2});

            var newService = new GitSyncActionService(m_Group);

            Assert.Equal(2, newService[SyncActionState.Active].Count());
            Assert.Single(newService[SyncActionState.Active, action1.FilePath]);
            Assert.Single(newService[SyncActionState.Active, action2.FilePath]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Add() throws " + nameof(DuplicateSyncActionException) + " if action already exists")]
        public void Add_throws_DuplicateSyncActionException_if_action_already_exists()
        {
            var action1 = new AddFileSyncAction(Guid.NewGuid(), "target", SyncActionState.Active, new FileReference("/path/to/file"));
            var action2 = new AddFileSyncAction(action1.Id, "target", SyncActionState.Active, new FileReference("/path/to/file"));

            m_Service.Add(new[] {action1});
            Assert.Throws<DuplicateSyncActionException>(() => m_Service.Add(new[] {action2}));
        }

        #endregion


        #region Update

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Update() updates actions")]
        public void Update_updates_actions()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Queued, new FileReference("/path/to/file"));
            var updatedAction = new AddFileSyncAction(action.Id, "target1", SyncActionState.Active, new FileReference("/path/to/file"));
            
            m_Service.Add(action);
            Assert.Single(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);

            m_Service.Update(updatedAction);
            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Single(m_Service[SyncActionState.Active]);
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Update() throws " + nameof(SyncActionNotFoundException) + " if action does not exist")]
        public void Update_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var updatedAction = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.Update(updatedAction));
        }

        #endregion


        #region Remove

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Remove() throws " + nameof(SyncActionNotFoundException) + " if action does not exist")]
        public void Remove_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var removedAction = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.Remove(removedAction));
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Remove() throws " + nameof(SyncActionNotFoundException) + " if action with the same state does not exist")]
        public void Remove_throws_SyncActionNotFoundException_if_action_with_the_same_state_does_not_exist()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, new FileReference("/path/to/file"));
            var removedAction = new AddFileSyncAction(action.Id, "target1", SyncActionState.Queued, new FileReference("/path/to/file"));

            Assert.Throws<SyncActionNotFoundException>(() => m_Service.Update(removedAction));
        }

        [Fact(DisplayName = nameof(GitSyncActionService) + ".Remove() removes an action")]
        public void Remove_removes_an_action()
        {
            var action = new AddFileSyncAction(Guid.NewGuid(), "target1", SyncActionState.Active, new FileReference("/path/to/file"));

            m_Service.Add(action);
            Assert.Single(m_Service[action.State]);

            m_Service.Remove(action);
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