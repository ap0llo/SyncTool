// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class ValueNode<T>
    {
        public T Value { get; }

        public int Index { get; }

        public ISet<ValueNode<T>> Successors { get; }
        

        public ValueNode(T value, int index, IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }

            Value = value;
            Index = index;

            var nodeComparer = new NodeComparer<T>(valueComparer);
            Successors = new HashSet<ValueNode<T>>(nodeComparer);
        }
        



    }
}