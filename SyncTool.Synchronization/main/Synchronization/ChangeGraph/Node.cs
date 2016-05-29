// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public abstract class Node<T>
    {
        public int Index { get; }

        public ISet<ValueNode<T>> Successors { get; }

        protected Node(IEqualityComparer<T> valueComparer, int index)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }

            //TODO: Check if index is in allowed range

            var nodeComparer = new NodeComparer<T>(valueComparer);
            Successors = new HashSet<ValueNode<T>>(nodeComparer);

            Index = index;
        }
    }
}