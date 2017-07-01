using System;
using System.Linq;
using SyncTool.Common.Groups;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
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

        readonly IGroup m_Group;
        readonly GitSyncActionService m_Service;
        readonly GitRepository m_Repository;

        public GitSyncActionServiceTest()
        {
            m_Group = CreateGroup();
            m_Service = m_Group.GetService<GitSyncActionService>();
            m_Repository = m_Group.GetService<GitRepository>();
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
            m_Repository.Value.CreateBranch(GitSyncActionService.BranchName, m_Repository.Value.GetInitialCommit());
            Assert.Empty(m_Service.AllItems);
        }


        [Fact]
        public void AllItems_returns_expected_result()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/file1"));
            var action2 = SyncAction.CreateAddFileSyncAction("target2", SyncActionState.Cancelled, 42, new FileReference("/path/to/file2"));
            var action3 = SyncAction.CreateAddFileSyncAction("target3", SyncActionState.Active, 42, new FileReference("/path/to/file3"));
            
            m_Service.AddItems(action1, action2, action3);

            var allItems = m_Service.AllItems.ToList();
            Assert.Equal(3, allItems.Count());

            Assert.Single(allItems.Where(a => a.State == SyncActionState.Queued));            
            Assert.Single(allItems.Where(a => a.State == SyncActionState.Cancelled));
            Assert.Single(allItems.Where(a => a.State == SyncActionState.Active));

        }

        #endregion


        #region PendingItems


        [Fact]
        public void PendingItems_returns_empty_enumerable_if_branch_does_not_exits()
        {
            Assert.Empty(m_Service.PendingItems);
        }

        [Fact]
        public void PendingItems_returns_empty_enumerable_if_branch_is_empty()
        {
            m_Repository.Value.CreateBranch(GitSyncActionService.BranchName, m_Repository.Value.GetInitialCommit());
            Assert.Empty(m_Service.PendingItems);
        }


        [Fact]
        public void PendingItems_returns_expected_result()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/file1"));
            var action2 = SyncAction.CreateAddFileSyncAction("target2", SyncActionState.Cancelled, 42, new FileReference("/path/to/file2"));
            var action3 = SyncAction.CreateAddFileSyncAction("target3", SyncActionState.Active, 42, new FileReference("/path/to/file3"));
            var action4 = SyncAction.CreateAddFileSyncAction("target3", SyncActionState.Completed, 42, new FileReference("/path/to/file3"));

            m_Service.AddItems(action1, action2, action3, action4);

            var pendingItems = m_Service.PendingItems.ToList();
            Assert.Equal(2, pendingItems.Count());

            Assert.Single(pendingItems.Where(a => a.State == SyncActionState.Queued));            
            Assert.Single(pendingItems.Where(a => a.State == SyncActionState.Active));
        }

        #endregion  


        #region Indexer

        [Fact]
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

        [Fact]
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

        [Fact]
        public void Indexer_returns_empty_enumerable_if_branch_is_empty()
        {
            m_Repository.Value.CreateBranch(GitSyncActionService.BranchName, m_Repository.Value.GetInitialCommit());

            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/somePath"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/somePath"]);

            Assert.Empty(m_Service["/somePath"]);
        }


        [Fact]
        public void Indexer_returns_empty_enumerable_if_no_actions_in_the_specified_state_exist()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 23, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);
            Assert.Empty(m_Service[SyncActionState.Completed]);
        }

        [Fact]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_path_exist()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 23, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[action.Path]);
            Assert.Empty(m_Service["/some/path"]);            
        }

        [Fact]
        public void Indexer_returns_empty_enumerable_if_no_actions_for_the_specified_state_and_path_exist()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/file"));

            m_Service.AddItems(action);

            Assert.NotEmpty(m_Service[SyncActionState.Queued, action.Path]);

            Assert.Empty(m_Service[SyncActionState.Queued, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Active, "/some/path"]);
            Assert.Empty(m_Service[SyncActionState.Completed, "/some/path"]);            
        }

        [Fact]
        public void Indexer_returns_all_actions_for_the_specified_state()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/file"));
            var action2 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Equal(2, m_Service[SyncActionState.Queued].Count());            
        }

        [Fact]
        public void Indexer_returns_all_actions_for_the_specified_path()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued,42,  new FileReference("/path/to/file"));
            var action2 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued, 42, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Single(m_Service[action1.Path]);
            Assert.Single(m_Service[action2.Path]);
        }

        [Fact]
        public void Indexer_returns_all_actions_for_the_specified_state_and_path()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued,23, new FileReference("/path/to/file"));
            var action2 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued,23, new FileReference("/path/to/another/file"));

            m_Service.AddItems(action1, action2);

            Assert.Single(m_Service[SyncActionState.Queued, action1.Path]);
            Assert.Single(m_Service[SyncActionState.Queued, action2.Path]);
        }

        #endregion


        #region AddItems

        [Fact]
        public void AddItems_stores_new_actions()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Active, 23,new FileReference("/path/to/file"));
            var action2 = SyncAction.CreateAddFileSyncAction("target2", SyncActionState.Active, 23, new FileReference("/path/to/some/Other/file"));

            m_Service.AddItems(action1, action2);

            var newService = new GitSyncActionService(m_Repository);

            Assert.Equal(2, newService[SyncActionState.Active].Count());
            Assert.Single(newService[SyncActionState.Active, action1.Path]);
            Assert.Single(newService[SyncActionState.Active, action2.Path]);
        }

        [Fact]
        public void AddItems_throws_DuplicateSyncActionException_if_action_already_exists()
        {
            var action1 = SyncAction.CreateAddFileSyncAction("target", SyncActionState.Active, 23, new FileReference("/path/to/file"));
            var action2 = SyncAction.CreateAddFileSyncAction(action1.Id, "target", SyncActionState.Active, 42, new FileReference("/path/to/file"));

            m_Service.AddItems(action1);
            Assert.Throws<DuplicateSyncActionException>(() => m_Service.AddItems(action2));
        }

        #endregion


        #region UpdateItems

        [Fact]
        public void UpdateItems_updates_actions()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Queued,42, new FileReference("/path/to/file"));
            var updatedAction = SyncAction.CreateAddFileSyncAction(action.Id, "target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            
            m_Service.AddItems(action);
            Assert.Single(m_Service[SyncActionState.Queued]);
            Assert.Empty(m_Service[SyncActionState.Active]);

            m_Service.UpdateItems(updatedAction);
            Assert.Empty(m_Service[SyncActionState.Queued]);
            Assert.Single(m_Service[SyncActionState.Active]);
        }

        [Fact]
        public void UpdateItems_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var updatedAction = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.UpdateItems(updatedAction));
        }

        #endregion


        #region RemoveItems

        [Fact]
        public void RemoveItems_throws_SyncActionNotFoundException_if_action_does_not_exist()
        {
            var removedAction = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            Assert.Throws<SyncActionNotFoundException>(() => m_Service.RemoveItems(removedAction));
        }

        [Fact]
        public void RemoveItems_throws_SyncActionNotFoundException_if_action_with_the_same_state_does_not_exist()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Active,42, new FileReference("/path/to/file"));
            var removedAction = SyncAction.CreateAddFileSyncAction(action.Id, "target1", SyncActionState.Queued, 42,new FileReference("/path/to/file"));

            Assert.Throws<SyncActionNotFoundException>(() => m_Service.RemoveItems(removedAction));
        }

        [Fact]
        public void RemoveItems_removes_an_action()
        {
            var action = SyncAction.CreateAddFileSyncAction("target1", SyncActionState.Active, 42, new FileReference("/path/to/file"));

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