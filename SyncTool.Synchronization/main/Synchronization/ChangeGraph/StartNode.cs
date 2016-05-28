// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class StartNode<T>
    {
        public int Index { get; } = 0;

        public ISet<ValueNode<T>> Successors { get; }


        public StartNode(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }            

            var nodeComparer = new NodeComparer<T>(valueComparer);
            Successors = new HashSet<ValueNode<T>>(nodeComparer);
        }
    }
}