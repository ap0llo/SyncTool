using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class NodeComparer<T> : IEqualityComparer<ValueNode<T>>
    {
        readonly IEqualityComparer<T> m_ValueComparer;


        public NodeComparer(IEqualityComparer<T> valueComparer)
        {
            m_ValueComparer = valueComparer ?? throw new ArgumentNullException(nameof(valueComparer));
        }


        public bool Equals(ValueNode<T> x, ValueNode<T> y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null || y == null)
            {
                return false;
            }

            return x.Index == y.Index && m_ValueComparer.Equals(x.Value, y.Value);
        }

        public int GetHashCode(ValueNode<T> obj) => m_ValueComparer.GetHashCode(obj.Value);
    }
}