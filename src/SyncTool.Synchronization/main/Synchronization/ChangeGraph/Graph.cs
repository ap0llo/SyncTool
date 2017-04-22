using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class Graph<T> : IGraph<T>
    { 
        readonly IEqualityComparer<T> m_ValueComparer;        
        readonly IDictionary<T, ValueNode<T>> m_Nodes;


        public StartNode<T> StartNode { get; }

        public IEnumerable<ValueNode<T>> ValueNodes => m_Nodes.Values;

        

        public Graph(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;
            m_Nodes = new NullKeyDictionary<T, ValueNode<T>>(valueComparer);   

            StartNode = new StartNode<T>(valueComparer, 0);         
        }


        public void AddNode(T value)
        {           
            if (!m_Nodes.ContainsKey(value))
            {
                m_Nodes.Add(value, new ValueNode<T>(value, m_ValueComparer, 0));                
            }
        }


        public void AddEdge(T start, T end)
        {            
            AddNode(start);
            AddNode(end);

            var startNode = m_Nodes[start];
            var endNode = m_Nodes[end];

            startNode.Successors.Add(endNode);
        }

        public void AddEdgeFromStartNode(T value)
        {
            AddNode(value);

            var node = m_Nodes[value];
            StartNode.Successors.Add(node);
        }

        public bool Contains(T value) => m_Nodes.ContainsKey(value);



        public AcyclicGraph<T> ToAcyclicGraph()
        {
            var nodeIndices = new Dictionary<Node<T>, int>();
            ExecuteDfsNumbering(StartNode, nodeIndices, 1);

            // eecute dfs search starting from every node for the case that the graph is not connected
            foreach (var node in ValueNodes)
            {
                ExecuteDfsNumbering(node, nodeIndices, nodeIndices.Values.Max() +1);
            }


            var startNodeIndex = nodeIndices[StartNode];
            var newGraph = new AcyclicGraph<T>(m_ValueComparer, startNodeIndex, nodeIndices.Values.Max());

            foreach (var node in StartNode.Successors)
            {
                var nodeIndex = nodeIndices[node];
                newGraph.AddEdgeFromStartNode(node.Value, nodeIndex);
            }

            foreach (var node in ValueNodes)
            {
                var nodeIndex = nodeIndices[node];
                newGraph.AddNode(node.Value, nodeIndex);
                foreach (var successor in node.Successors)
                {
                    var successorIndex = nodeIndices[successor];
                    newGraph.AddEdge(node.Value, nodeIndex, successor.Value, successorIndex);
                }
            }
            
            return newGraph;
        }



        void ExecuteDfsNumbering(Node<T> currentNode, Dictionary<Node<T>, int> nodeIndices, int nextNodeId)
        {
            if (!nodeIndices.ContainsKey(currentNode))
            {
                nodeIndices.Add(currentNode, nextNodeId);

                foreach (var node in currentNode.Successors)
                {
                    ExecuteDfsNumbering(node, nodeIndices, nextNodeId + 1);
                }       
            }
        }

 
    }
}