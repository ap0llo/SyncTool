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