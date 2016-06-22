// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization.ChangeGraph;
using SyncTool.Synchronization.State;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.ChangeGraph
{
    /// <summary>
    /// Tests for <see cref="ChangeGraphService"/>
    /// Tests are located in SyncTool.Git so git implementations of the services can be used instead of mocking everything
    /// </summary>
    public class ChangeGraphServiceTest : GitGroupBasedTest
    {
        readonly GitBasedGroup m_Group;
        readonly ChangeGraphService m_Instance;


        public ChangeGraphServiceTest()
        {
            m_Group = CreateGroup();
            m_Instance = new ChangeGraphService(EqualityComparer<IFileReference>.Default, m_Group.GetHistoryService());
        }

        [Fact]
        public void GetChangeGraphs_throws_ArgumentException_if_from_argument_specifies_other_histories_than_to_argument_2()
        {
            var from = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", "Irrelevant"));
            var to = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", "Irrelevant"), new HistorySnapshotId("anotherHistory", "Ireelevant"));

            Assert.Throws<ArgumentException>(() => m_Instance.GetChangeGraphs(from, to));            
            Assert.Throws<ArgumentException>(() => m_Instance.GetChangeGraphs(to, from));            
        }

        [Fact]
        public void GetChangeGraphs_compares_history_names_case_insensitive()
        {
            var from = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", "Irrelevant"));
            var to = new HistorySnapshotIdCollection(new HistorySnapshotId("hiSTOry1", "Irrelevant"));

            m_Instance.GetChangeGraphs(from, to);
            m_Instance.GetChangeGraphs(to, from);
        }

        [Fact]
        public void GetChangeGraphs_throws_ArgumentException_if_empty_set_of_snapshots_is_specified()
        {
            var notEmpty = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", "Irrelevant"));            

            Assert.Throws<ArgumentException>(() => m_Instance.GetChangeGraphs(new HistorySnapshotIdCollection()));
            Assert.Throws<ArgumentException>(() => m_Instance.GetChangeGraphs(new HistorySnapshotIdCollection(), notEmpty));
            Assert.Throws<ArgumentException>(() => m_Instance.GetChangeGraphs(notEmpty, new HistorySnapshotIdCollection()));
        }

        [Fact]
        public void GetChangeGraphs_does_not_include_changes_not_included_by_the_path_filter()
        {
            // ARRANGE
            var historyBuilder = new HistoryBuilder(m_Group, "history1");
            historyBuilder.AddFile("file1");
            historyBuilder.AddFile("file2");
            var snapshot = historyBuilder.CreateSnapshot();
            var snapshotIds = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", snapshot.Id));

            //ACT
            var graphs = m_Instance.GetChangeGraphs(snapshotIds, new [] { "/file1" }).ToArray();

            //ASSERT
            Assert.Single(graphs);

            //expected: two nodes in the change graph: [null] -> file1 
            var nodes = graphs.Single().ValueNodes.ToArray();
            Assert.Equal(2, nodes.Length);
            Assert.Single(nodes.Where(n => n.Value == null));
            Assert.Single(nodes.Where(n => n.Value != null).Where(n => n.Value.Matches(historyBuilder.CurrentState.GetFile("file1"))));            
            Assert.Single(nodes.Single(n => n.Value == null).Successors);
            Assert.Empty(nodes.Single(n => n.Value != null).Successors);
        }

        [Fact]
        public void GetChangeGraphs_does_not_include_changes_between_snapshots_not_included_by_the_path_filter()
        {
            // ARRANGE
            var historyBuilder = new HistoryBuilder(m_Group, "history1");
            historyBuilder.AddFile("file1");
            var snapshot1 = historyBuilder.CreateSnapshot();
            historyBuilder.AddFile("file2");
            var snapshot2 = historyBuilder.CreateSnapshot();
            var from = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", snapshot1.Id));
            var to = new HistorySnapshotIdCollection(new HistorySnapshotId("history1", snapshot2.Id));

            //ACT
            var graphs = m_Instance.GetChangeGraphs(from, to, new[] { "/file1" }).ToArray();

            //ASSERT
            Assert.Single(graphs);
            //expected: two nodes in the change graph: [null] -> file2w
            var nodes = graphs.Single().ValueNodes.ToArray();
            Assert.Equal(2, nodes.Length);
            Assert.Single(nodes.Where(n => n.Value == null));
            Assert.Single(nodes.Where(n => n.Value != null).Where(n => n.Value.Matches(historyBuilder.CurrentState.GetFile("file2"))));
            Assert.Single(nodes.Single(n => n.Value == null).Successors);
            Assert.Empty(nodes.Single(n => n.Value != null).Successors);
        }

        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}