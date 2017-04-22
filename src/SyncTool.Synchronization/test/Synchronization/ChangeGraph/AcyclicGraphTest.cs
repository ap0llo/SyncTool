using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SyncTool.Synchronization.ChangeGraph
{

    /// <summary>
    /// Tests for <see cref="AcyclicGraph{T}"/>
    /// </summary>
    public class AcyclicGraphTest
    {

        [Fact]
        public void Graph_does_not_contain_edges()
        {
            var graph = new AcyclicGraph<string>(EqualityComparer<string>.Default, 1, 3);

            // Start -> A(2)
            // A(2) -> B(3)
            // B(3) -> A(2)

            graph.AddEdgeFromStartNode("A", 2);
            graph.AddEdge("A", 2, "B", 3);
            graph.AddEdge("B", 3, "A", 2);

            // expected graph: Start -> A -> B -> A'

            Assert.Equal(3, graph.ValueNodes.Count());

            Assert.Single(graph.StartNode.Successors);
            Assert.Single(graph.StartNode.Successors.Single().Successors);
            Assert.Single(graph.StartNode.Successors.Single().Successors.Single().Successors);

        } 

    }
}