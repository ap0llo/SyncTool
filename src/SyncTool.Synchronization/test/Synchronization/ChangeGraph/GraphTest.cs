// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SyncTool.Synchronization.ChangeGraph
{
    /// <summary>
    /// Tests for <see cref="Graph{T}"/>
    /// </summary>
    public class GraphTest
    {
        [Fact]
        public void Graph_can_contain_null()
        {
            var graph = new Graph<object>(EqualityComparer<object>.Default);

            var obj = new object();
            graph.AddNode(null);
            graph.AddNode(obj);

            graph.AddEdge(null, obj);

            Assert.Equal(2, graph.ValueNodes.Count());
        }

        [Fact]
        public void Graph_can_be_converted_to_acyclic_graph_01()
        {
            var graph = new Graph<int>(EqualityComparer<int>.Default);            

            graph.AddEdgeFromStartNode(0);
            graph.AddEdge(0,1);
            graph.AddEdge(1,2);
            graph.AddEdge(2,1);

            Assert.Equal(4, graph.ToAcyclicGraph().ValueNodes.Count());

        }

        [Fact]
        public void Graph_can_be_converted_to_acyclic_graph_02()
        {
            // ARRANGE
            var expectedValuesInOrder = new[] {0, 1, 2, 1};
            var graph = new Graph<int>(EqualityComparer<int>.Default);

            // ACT
            graph.AddEdgeFromStartNode(0);            
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 2);
            graph.AddEdge(2, 1);

            var nodes = graph.ToAcyclicGraph().ValueNodes.ToArray();

            // ASSERT
            Assert.Equal(4, nodes.Length);
            Assert.Equal(expectedValuesInOrder, nodes.Select(n => n.Value).ToArray());

            // expected Graph 0 -> 1 -> 2 -> 1 (1 was added twice to prevent a cycle)
            Assert.Equal(1, nodes[0].Successors.Single().Value);

            Assert.Equal(2, nodes[1].Successors.Single().Value);

            Assert.Equal(1, nodes[2].Successors.Single().Value);

            Assert.Empty(nodes[3].Successors);
        }

        [Fact]
        public void Graph_can_be_converted_to_acyclic_graph_03()
        {
            // ARRANGE
            var expectedValuesInOrder = new[] { 0, 1, 2, 0 };
            var graph = new Graph<int>(EqualityComparer<int>.Default);

            // ACT
            graph.AddEdgeFromStartNode(0);            
            graph.AddEdge(0, 1);
            graph.AddEdge(1, 0);
            graph.AddEdge(2, 0);

            var nodes = graph.ToAcyclicGraph().ValueNodes.ToArray();

            // ASSERT
            Assert.Equal(4, nodes.Length);
            Assert.Equal(expectedValuesInOrder, nodes.Select(n => n.Value).ToArray());

            // expected Graph 
            //            2
            //            |
            //            v
            //  0 -> 1 -> 0

            var node0_1 = nodes.First(n => n.Value == 0);
            var node0_2 = nodes.Last(n => n.Value == 0);
            var node1 = nodes.Single(n => n.Value == 1);
            var node2 = nodes.Single(n => n.Value == 2);
            
            Assert.Single(node2.Successors);
            Assert.Equal(node0_2, node2.Successors.Single());

            Assert.Single(node0_1.Successors);
            Assert.Equal(node1, node0_1.Successors.Single());

            Assert.Single(node1.Successors);
            Assert.Equal(node0_2, node1.Successors.Single());

            Assert.Empty(node0_2.Successors);            
        }
    }
}