// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization.State;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.State
{
    /// <summary>
    /// Tests for <see cref="GitSynchronizationStateService" />
    /// </summary>
    public class GitSynchronizationStateServiceTest : GitGroupBasedTest
    {
        readonly GitBasedGroup m_Group;
        readonly GitSynchronizationStateService m_Service;

        public GitSynchronizationStateServiceTest()
        {
            m_Group = CreateGroup();
            m_Service = new GitSynchronizationStateService(m_Group);
        }


        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ".Items is empty for empty repository")]
        public void Items_is_empty_for_empty_repository()
        {
            Assert.Empty(m_Service.Items);
        }

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ".AddSynchronizationState() stores the state")]
        public void AddSynchronizationState_stores_the_state()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(1)
                .WithoutFromSnapshots()
                .WithToSnapshot("snapshot1", "value1")
                .Object;


            m_Service.AddSynchronizationState(state);

            Assert.Single(m_Service.Items);
            Assert.True(m_Service.ItemExists(1));
            Assert.Equal(state, m_Service[1]);
        }

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ".AddSynchronizationState() throws " + nameof(ArgumentNullException) + " if state is null")]
        public void AddSynchronizationState_throws_ArgumentNullException_if_state_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service.AddSynchronizationState(null));
        }

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ".AddSynchronizationState() throws " + nameof(DuplicateSynchronizationStateException) + " if state id already exists")]
        public void AddSynchronizationState_throws_DuplicateSynchronizationStateException_if_state_id_already_exists()
        {
            var state1 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(1)
                .WithoutFromSnapshots()
                .WithToSnapshot("name", "id")
                .Object;

            var state2 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(1)
                .WithoutFromSnapshots()
                .WithToSnapshot("name", "id")
                .Object;

            m_Service.AddSynchronizationState(state1);
            Assert.Throws<DuplicateSynchronizationStateException>(() => m_Service.AddSynchronizationState(state2));
        }
        
        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ".AddSynchronizationState() correctly stores multiple states")]
        public void AddSynchronizationState_correctly_stores_multiple_states()
        {
            var state1 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(1)
                .WithoutFromSnapshots()
                .WithToSnapshot("name", "id")
                .Object;

            var state2 = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(2)
                .WithoutFromSnapshots()
                .WithToSnapshot("name", "id")
                .Object;

            m_Service.AddSynchronizationState(state1);
            m_Service.AddSynchronizationState(state2);

            Assert.Equal(2, m_Service.Items.Count());
            Assert.True(m_Service.ItemExists(1));
            Assert.True(m_Service.ItemExists(2));
        }

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ": States from the repository are loaded correclty")]
        public void Values_from_disk_are_loaded_correctly()
        {
            var state1 = new MutableSynchronizationState()
            {
                Id = 1,
                FromSnapshots = null,
                ToSnapshots = new Dictionary<string, string>()
                {
                    {"name1", "id1" }
                }
            };

            var state2 = new MutableSynchronizationState()
            {
                Id = 2,
                FromSnapshots = new Dictionary<string, string>()
                {
                    {"name2", "id2"}
                },
                ToSnapshots = new Dictionary<string, string>()
                {
                    {"name3", "id3"}
                }
            };

            m_Service.AddSynchronizationState(state1);
            m_Service.AddSynchronizationState(state2);


            // create another service instance that needs to load the state from disk

            var service  = new GitSynchronizationStateService(m_Group);

            Assert.Equal(2, service.Items.Count());
            Assert.True(service.ItemExists(1));
            Assert.True(service.ItemExists(2));

            {
                var state1RoundTrip = service[1];

                Assert.Equal(state1.Id, state1RoundTrip.Id);

                Assert.Null(state1RoundTrip.FromSnapshots);

                Assert.NotNull(state1RoundTrip.ToSnapshots);
                Assert.Single(state1RoundTrip.ToSnapshots);
                Assert.Equal("id1", state1RoundTrip.ToSnapshots["name1"]);
            }
            {

                var state2RoundTrip = service[2];

                Assert.Equal(state2.Id, state2RoundTrip.Id);

                Assert.NotNull(state2RoundTrip.FromSnapshots);
                Assert.Single(state2RoundTrip.FromSnapshots);
                Assert.Equal("id2", state2RoundTrip.FromSnapshots["name2"]);

                Assert.NotNull(state2RoundTrip.ToSnapshots);
                Assert.Single(state2RoundTrip.ToSnapshots);
                Assert.Equal("id3", state2RoundTrip.ToSnapshots["name3"]);
            }


        }       

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ": Indexer throws " + nameof(SynchronizationStateNotFoundException) + " for unknown state")]
        public void Indexer_throws_SynchronizationStateNotFoundException_for_unknown_state()
        {
            Assert.Throws<SynchronizationStateNotFoundException>(() => m_Service[12]);
        }

        [Fact(DisplayName = nameof(GitSynchronizationStateService) + ": Indexer returns expected state")]
        public void Indexer_returns_expected_state()
        {
            var state = SynchronizationStateMockingHelper.GetSynchronizationStateMock()
                .WithId(1)
                .WithoutFromSnapshots()
                .WithToSnapshot("name", "id")
                .Object;

            m_Service.AddSynchronizationState(state);
            Assert.Equal(state, m_Service[1]);
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}