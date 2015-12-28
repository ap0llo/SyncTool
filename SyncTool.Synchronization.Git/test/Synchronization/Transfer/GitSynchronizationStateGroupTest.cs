// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using Moq;
using SyncTool.Common;
using SyncTool.FileSystem.Git.Utilities;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization.Transfer
{
    /// <summary>
    ///     Tests for <see cref="GitSynchronizationStateGroup" />
    /// </summary>
    public class GitSynchronizationStateGroupTest : DirectoryBasedTest
    {
        readonly GitSynchronizationStateGroup m_Group;


        public GitSynchronizationStateGroupTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);
            m_Group = new GitSynchronizationStateGroup(m_TempDirectory.Location);
        }

        [Fact]
        public void Items_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Group.Items);
        }

        [Fact]
        public void Items_returns_expected_number_of_elements()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;

            m_Group.SetState("state1", state);
            Assert.Single(m_Group.Items);

            m_Group.SetState("state2", state);
            Assert.Equal(2, m_Group.Items.Count());
        }

        [Fact]
        public void SetState_creates_a_new_state_if_the_state_does_not_yet_exist()
        {
            Assert.Empty(m_Group.Items);

            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;
            m_Group.SetState("state1", state);

            Assert.Single(m_Group.Items);

        }

        [Fact]
        public void SetState_overwrites_existing_state_if_state_with_specified_name_already_exists()
        {
            var state1 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithEmptyActionLists()
                .WithIds("global1", "local1")
                .Object;

            m_Group.SetState("state", state1);

            Assert.Single(m_Group.Items);
            Assert.Equal("global1", m_Group.Items.Single().GlobalSnapshotId);
            Assert.Equal("local1", m_Group.Items.Single().LocalSnapshotId);

            var state2 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithEmptyActionLists()
                .WithIds("global2", "local2")
                .Object;

            // state names must be handled case-invariant
            m_Group.SetState("StATE", state2);

            var gitState = m_Group.Items.Single();

            Assert.Equal("global2", gitState.GlobalSnapshotId);
            Assert.Equal("local2", gitState.LocalSnapshotId);
        }

        [Fact]
        public void GetItem_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            Assert.Throws<ArgumentNullException>(() => m_Group.GetItem(null));
            Assert.Throws<ArgumentNullException>(() => m_Group.GetItem(""));
            Assert.Throws<ArgumentNullException>(() => m_Group.GetItem(" "));
        }

        [Fact]
        public void GetItem_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {
            Assert.Throws<ItemNotFoundException>(() => m_Group.GetItem("Irrelevant"));
        }

        [Fact]
        public void GetItem_returns_expected_item()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;
            m_Group.SetState("item1", state);

            Assert.NotNull(m_Group.GetItem("item1"));
            Assert.NotNull(m_Group.GetItem("ITem1"));
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }

      

    }
}