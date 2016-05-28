// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class Graph<T>
    { 
        readonly IEqualityComparer<T> m_ValueComparer;        
        readonly IDictionary<T, IDictionary<int, Node<T>>> m_Nodes;            
        int m_NextNodeIndex = 1;
        bool m_EdgesAdded = false;

        public IEnumerable<Node<T>> Nodes => m_Nodes.Values.SelectMany(x => x.Values).OrderBy(x => x.Index);


        public Graph(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;           
            m_Nodes = new NullKeyDictionary<T, IDictionary<int, Node<T>>>(valueComparer);            
        }

        public void AddNodes(params T[] values) => AddNodes((IEnumerable<T>) values);
        
        public void AddNodes(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                AddNode(value);
            }
        }

        public void AddNode(T value)
        {
            if (m_EdgesAdded)
            {
                throw new InvalidOperationException("New nodes cannot be added, after edges were added to the graph");
            }

            if (!m_Nodes.ContainsKey(value))
            {
                m_Nodes.Add(value, new Dictionary<int, Node<T>>());
                var newNode = CreateNode(value);
                m_Nodes[value].Add(newNode.Index, newNode);
            }
        }

        public void AddEdge(T start, T end)
        {
            m_EdgesAdded = true;

            var startNode = m_Nodes[start][m_Nodes[start].Keys.Min()];
            var endNode = m_Nodes[end][m_Nodes[end].Keys.Max()];

            if (startNode.Index >= endNode.Index)
            {
                //Add new node for end value to prevent cycles
                endNode = CreateNode(end);
                m_Nodes[end].Add(endNode.Index, endNode);                
            }

            startNode.Successors.Add(endNode);
        }        

        public bool Contains(T value) => m_Nodes.ContainsKey(value);

        
        Node<T> CreateNode(T value)
        {
            return new Node<T>(value, m_NextNodeIndex++, m_ValueComparer);
        }

    }
}