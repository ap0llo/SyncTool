// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using SyncTool.Synchronization.ChangeGraph;

namespace SyncTool.Synchronization.ChangeGraph
{
    public static class GraphExtensions
    {
        public static IEnumerable<T> GetSinks<T>(this Graph<T> graph)
        {
            return from node in graph.ValueNodes
                   where !node.Successors.Any()
                   select node.Value;
        }



        public static void AddNodes<T>(this Graph<T> graph, params T[] values) => graph.AddNodes((IEnumerable<T>)values);

        public static void AddNodes<T>(this Graph<T> graph, IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                graph.AddNode(value);
            }
        }

    }
}