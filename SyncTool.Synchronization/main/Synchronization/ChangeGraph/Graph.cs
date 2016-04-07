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
        readonly IDictionary<T, Node<T>> m_Nodes;
        bool m_ContainsNull = false;
        readonly Node<T> m_NullNode;

        public IEnumerable<Node<T>> Nodes
        {
            get
            {
                if (m_ContainsNull)
                {
                    return m_Nodes.Values.Union(new[]{m_NullNode});                    
                }
                else
                {
                    
                return m_Nodes.Values;
                }
                
            }
        }


        public Graph(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;           
            m_Nodes = new Dictionary<T, Node<T>>(valueComparer);
            m_NullNode = new Node<T>((T)(object)null, valueComparer);
        }


        public void AddEdge(T start, T end)
        {
            var startNode = GetNodeForValue(start);
            var endNode = GetNodeForValue(end);

            startNode.Successors.Add(endNode);
            endNode.Predecessors.Add(startNode);
        }
        

        public bool Contains(T value) => value == null ? m_ContainsNull : m_Nodes.ContainsKey(value);



        private Node<T> GetNodeForValue(T value)
        {
            if (value == null)
            {
                m_ContainsNull = true;
                return m_NullNode;
            }
            else
            {
                if (!m_Nodes.ContainsKey(value))
                {
                    m_Nodes.Add(value, new Node<T>(value, m_ValueComparer));
                }
                return m_Nodes[value];
            }
        }




    }
}