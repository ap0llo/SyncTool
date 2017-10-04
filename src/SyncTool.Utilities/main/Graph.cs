using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Utilities
{
    /// <summary>
    /// Generic directed graph data structure
    /// </summary>
    public sealed class Graph<TNode, TEdge>
    {
        readonly object m_Lock = new object();
        readonly IEqualityComparer<TNode> m_NodeComparer;
        readonly IEqualityComparer<TEdge> m_EdgeComparer;
        readonly HashSet<(TNode, TNode, TEdge)> m_Edges;
        readonly IDictionary<TNode, HashSet<TNode>> m_Successors;
        readonly IDictionary<TNode, HashSet<TNode>> m_Predecessors;


        public IEnumerable<TNode> Nodes => m_Successors.Keys;

        public IEnumerable<(TNode from, TNode to, TEdge data)> Edges => m_Edges;


        public Graph() : this(EqualityComparer<TNode>.Default, EqualityComparer<TEdge>.Default)
        {
        }

        public Graph(IEqualityComparer<TNode> nodeComparer) : this(nodeComparer, EqualityComparer<TEdge>.Default)
        {
        }

        public Graph(IEqualityComparer<TNode> nodeComparer, IEqualityComparer<TEdge> edgeComparer)
        {
            m_NodeComparer = nodeComparer ?? throw new ArgumentNullException(nameof(nodeComparer));
            m_EdgeComparer = edgeComparer ?? throw new ArgumentNullException(nameof(edgeComparer));
     
            m_Edges = new HashSet<(TNode, TNode, TEdge)>(TupleComparer.Create(m_NodeComparer, m_NodeComparer, m_EdgeComparer));
            m_Successors = new NullKeyDictionary<TNode, HashSet<TNode>>(m_NodeComparer);
            m_Predecessors = new NullKeyDictionary<TNode, HashSet<TNode>>(m_NodeComparer);
        }
        

        public bool AddNode(TNode node)
        {
            lock (m_Lock)
            {               
                if (m_Successors.ContainsKey(node))
                    return false;

                m_Successors.Add(node, new HashSet<TNode>(m_NodeComparer));
                m_Predecessors.Add(node, new HashSet<TNode>(m_NodeComparer));

                return true;
            }
        }

        public bool AddEdge(TNode from, TNode to, TEdge edgeData)
        {
            lock (m_Lock)
            {                
                if (!ContainsNode(from))
                    throw new KeyNotFoundException($"Could not find node {from}");

                if (!ContainsNode(to))
                    throw new KeyNotFoundException($"Could not find node {to}");

                m_Successors[from].Add(to);
                m_Predecessors[to].Add(from);

                return m_Edges.Add((from, to, edgeData));
            }
        }

        public bool ContainsNode(TNode node) => m_Successors.ContainsKey(node);

        public bool ContainsEdge(TNode from, TNode to, TEdge edgeData) => m_Edges.Contains((from, to, edgeData));

        public IReadOnlyCollection<TNode> GetSuccessors(TNode node) => m_Successors[node];

        public IReadOnlyCollection<TNode> GetPredecessors(TNode node) => m_Predecessors[node];

        public bool IsSink(TNode node) => GetSuccessors(node).Count == 0;

        public bool IsSource(TNode node) => GetPredecessors(node).Count == 0;

        public IReadOnlyCollection<TNode> GetSinks() => Nodes.Where(IsSink).ToArray();

        public IReadOnlyCollection<TNode> GetSources() => Nodes.Where(IsSource).ToArray();
    }
}
