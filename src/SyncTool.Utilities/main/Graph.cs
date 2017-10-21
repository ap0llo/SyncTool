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
        readonly HashSet<(TNode from, TNode to, TEdge data)> m_Edges;
        readonly IDictionary<TNode, HashSet<TNode>> m_Successors;
        readonly IDictionary<TNode, HashSet<TNode>> m_Predecessors;


        public IEnumerable<TNode> Nodes => m_Successors.Keys;

        public IEnumerable<(TNode from, TNode to, TEdge data)> Edges => m_Edges;

        /// <summary>
        /// Determines if the graph contains cycles by topologically sorting the nodes
        /// </summary>
        /// <returns></returns>
        public bool HasCycles
        {
            get
            {
                // see https://stackoverflow.com/questions/4168/graph-serialization/4577#4577

                // L ← Empty list where we put the sorted elements
                var l = new HashSet<TNode>(m_NodeComparer);
                // Q ← Set of all nodes with no incoming edges
                var q = GetSources().ToHashSet(m_NodeComparer);
                HashSet<(TNode from, TNode to, TEdge data)> edges = Edges.ToHashSet(TupleComparer.Create(m_NodeComparer, m_NodeComparer, m_EdgeComparer));

                // while Q is non-empty do
                while (q.Any())
                {
                    // remove a node n from Q
                    // insert n into L
                    var n = q.First();
                    q.Remove(n);
                    l.Add(n);


                    // for each node m with an edge e (n -> m) from n to m do
                    var edgesToRemove = edges.Where(e => m_NodeComparer.Equals(e.from, n)).ToArray();
                    foreach (var e in edgesToRemove)
                    {
                        var m = e.to;
                        // remove edge e from the graph
                        edges.Remove(e);

                        //if m has no other incoming edges then
                        var hasEdges = edges.Any(x => m_NodeComparer.Equals(x.to, m));
                        if (!hasEdges)
                        {
                            //insert m into Q
                            q.Add(m);
                        }
                    }
                }

                // if graph has edges then the graph has cycles
                return edges.Any();
            }
        }


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
