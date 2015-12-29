// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.Git.Common;
using SyncTool.Git.Synchronization.Transfer;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.Transfer
{
    /// <summary>
    /// Tests for <see cref="GitSynchronizationState"/>
    /// </summary>
    public class GitSynchronizationStateTest : DirectoryBasedTest
    {
        const string s_BranchName = "stateBranch";
        readonly Repository m_Repository;

        public GitSynchronizationStateTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);
            m_Repository = new Repository(m_TempDirectory.Location);
            m_Repository.CreateBranch(s_BranchName, m_Repository.GetAllCommits().Single());
        }


        [Fact]
        public void Create_creates_a_SynchronizationState_in_the_underlying_directory()
        {
            var inputState = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithEmptyActionLists()
                .WithIds("id1", "id2")
                .Object;

            var gitState = GitSynchronizationState.Create(m_Repository, s_BranchName, inputState);

            Assert.Equal("id1", gitState.GlobalSnapshotId);
            Assert.Equal("id2", gitState.LocalSnapshotId);
        }


        [Fact]
        public void Create_creates_a_SynchronizationState_in_the_and_stores_queued_actions()
        {            
            var file = new File(new NullDirectory("", "root"), "file1");

            var inputState = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
               .WithEmptyActionLists()
               .WithQueuedActions(new AddFileSyncAction(SyncParticipant.Right,file))
               .WithIds("id1", "id2")
               .Object;

            var gitState = GitSynchronizationState.Create(m_Repository, s_BranchName, inputState);

            Assert.Empty(gitState.CompletedActions);
            Assert.Empty(gitState.InProgressActions);
            Assert.Single(gitState.QueuedActions);
            Assert.Equal(SyncParticipant.Right, gitState.QueuedActions.Single().Target);
        }

        [Fact]
        public void Create_creates_a_SynchronizationState_in_the_and_stores_completed_actions()
        {
            var file = new File(new NullDirectory("", "root"), "file1");

            var inputState = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
               .WithEmptyActionLists()
               .WithCompletedActions(new AddFileSyncAction(SyncParticipant.Right, file))
               .WithIds("id1", "id2")
               .Object;

            var gitState = GitSynchronizationState.Create(m_Repository, s_BranchName, inputState);

            Assert.Empty(gitState.QueuedActions);
            Assert.Empty(gitState.InProgressActions);
            Assert.Single(gitState.CompletedActions);
            Assert.Equal(SyncParticipant.Right, gitState.CompletedActions.Single().Target);
        }

        [Fact]
        public void Create_creates_a_SynchronizationState_in_the_and_stores_inprogress_actions()
        {
            var file = new File(new NullDirectory("", "root"), "file1");

            var inputState = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
               .WithEmptyActionLists()
               .WithInProgressActions(new AddFileSyncAction(SyncParticipant.Right, file))
               .WithIds("id1", "id2")
               .Object;

            var gitState = GitSynchronizationState.Create(m_Repository, s_BranchName, inputState);

            Assert.Empty(gitState.QueuedActions);
            Assert.Empty(gitState.CompletedActions);
            Assert.Single(gitState.InProgressActions);
            Assert.Equal(SyncParticipant.Right, gitState.InProgressActions.Single().Target);
        }

        public override void Dispose()
        {
            m_Repository.Dispose();
            base.Dispose();
        }
    }
}