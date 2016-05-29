// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class AcyclicGraph<T> : IGraph<T>
    {
        readonly IEqualityComparer<T> m_ValueComparer;
        readonly int m_StartNodeIndex;
        readonly IDictionary<T, Dictionary<int, ValueNode<T>>> m_ValueNodes;        
        int m_NextNodeIndex;

        public StartNode<T> StartNode { get; }

        public IEnumerable<ValueNode<T>> ValueNodes => m_ValueNodes.Keys.SelectMany(key => m_ValueNodes[key].Values).OrderBy(x => x.Index).ToArray();


        public AcyclicGraph(IEqualityComparer<T> valueComparer, int startNodeIndex, int maxIndex)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;
            m_StartNodeIndex = startNodeIndex;
            m_ValueNodes = new NullKeyDictionary<T, Dictionary<int, ValueNode<T>>>(valueComparer);            

            m_NextNodeIndex = maxIndex + 1;

            StartNode = new StartNode<T>(valueComparer, startNodeIndex);


            //TODO: check that maxIndex > startNodeIndex
        }


        public void AddNode(T value, int index)
        {
            if (index <= m_StartNodeIndex)
            {
                throw new ArgumentException("A node's index must not be less or equal than the index of the start node");
            }

            if (!m_ValueNodes.ContainsKey(value))
            {
                m_ValueNodes.Add(value, new Dictionary<int, ValueNode<T>>());
                var node = new ValueNode<T>(value, m_ValueComparer, index);
                m_ValueNodes[value].Add(index, node);
            }
        }

        public void AddEdge(T start, int startIndex, T end, int endIndex)
        {
            AddNode(start, startIndex);
            AddNode(end, endIndex);

            var startNode = m_ValueNodes[start][m_ValueNodes[start].Keys.Min()];
            var endNode = m_ValueNodes[end][m_ValueNodes[end].Keys.Max()];

            if (startNode.Index >= endNode.Index)
            {
                //Add new node for end value to prevent cycles
                endNode = CreateNode(end);
                m_ValueNodes[end].Add(endNode.Index, endNode);                
            }

            startNode.Successors.Add(endNode);
        }

        public void AddEdgeFromStartNode(T value, int index)
        {
            AddNode(value, index);

            var node = m_ValueNodes[value][m_ValueNodes[value].Keys.Max()];
            StartNode.Successors.Add(node);
        }


        ValueNode<T> CreateNode(T value)
        {
            return new ValueNode<T>(value, m_ValueComparer, m_NextNodeIndex++);
        }

    }
}