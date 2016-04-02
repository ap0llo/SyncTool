// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class Graph<T>
    {       
        readonly IEqualityComparer<T> m_ValueComparer;        
        readonly IDictionary<T, Node<T>> m_Nodes;


        public IEnumerable<Node<T>> Nodes => m_Nodes.Values;


        public Graph(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;
            m_Nodes = new Dictionary<T, Node<T>>(valueComparer);            
        }


        public void AddEdge(T start, T end)
        {
            if (!m_Nodes.ContainsKey(start))
            {
                m_Nodes.Add(start, new Node<T>(start, m_ValueComparer));
            }

            if (m_Nodes.ContainsKey(end))
            {
                m_Nodes.Add(end, new Node<T>(end, m_ValueComparer));
            }

            var startNode = m_Nodes[start];
            var endNode = m_Nodes[end];

            startNode.Successors.Add(endNode);
            endNode.Predecessors.Add(startNode);
        }
    }
}