// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.Transfer
{
    /// <summary>
    ///     Tests for <see cref="GitSynchronizationStateService" />
    /// </summary>
    public class GitSynchronizationStateServiceTest : DirectoryBasedTest
    {
        readonly GitBasedGroup m_Group;
        readonly GitSynchronizationStateService m_Service;

        public GitSynchronizationStateServiceTest()
        {
            RepositoryInitHelper.InitializeRepository(m_TempDirectory.Location);
            m_Group = new GitBasedGroup(m_TempDirectory.Location);
            m_Service = new GitSynchronizationStateService(m_Group);
        }

        [Fact]
        public void Items_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Service.Items);
        }

        [Fact]
        public void Items_returns_expected_number_of_elements()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;

            m_Service["state1"] = state;
            Assert.Single(m_Service.Items);

            m_Service["state2"] = state;
            Assert.Equal(2, m_Service.Items.Count());
        }

        [Fact]
        public void Indexer_Set_creates_a_new_state_if_the_state_does_not_yet_exist()
        {
            Assert.Empty(m_Service.Items);

            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;
            m_Service["state1"] = state;

            Assert.Single(m_Service.Items);

        }

        [Fact]
        public void Indexer_Set_overwrites_existing_state_if_state_with_specified_name_already_exists()
        {
            var state1 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithEmptyActionLists()
                .WithIds("global1", "local1")
                .Object;

            m_Service["state"] = state1;

            Assert.Single(m_Service.Items);
            Assert.Equal("global1", m_Service.Items.Single().GlobalSnapshotId);
            Assert.Equal("local1", m_Service.Items.Single().LocalSnapshotId);

            var state2 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithEmptyActionLists()
                .WithIds("global2", "local2")
                .Object;

            // state names must be handled case-invariant
            m_Service["StATE"] = state2;

            var gitState = m_Service.Items.Single();

            Assert.Equal("global2", gitState.GlobalSnapshotId);
            Assert.Equal("local2", gitState.LocalSnapshotId);
        }

        [Fact]
        public void Indexer_Get_throws_ArgumentNullException_if_name_is_null_or_whitespace()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service[null]);
            Assert.Throws<ArgumentNullException>(() => m_Service[""]);
            Assert.Throws<ArgumentNullException>(() => m_Service[" "]);
        }

        [Fact]
        public void Indexer_Get_throws_ItemNotFoundException_if_requested_item_could_not_be_found()
        {
            Assert.Throws<ItemNotFoundException>(() => m_Service["Irrelevant"]);
        }

        [Fact]
        public void Indexer_Get_returns_expected_item()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock().WithEmptyActionLists().WithIds().Object;
            m_Service["item1"] =  state;

            Assert.NotNull(m_Service["item1"]);
            Assert.NotNull(m_Service["ITem1"]);
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }

      

    }
}