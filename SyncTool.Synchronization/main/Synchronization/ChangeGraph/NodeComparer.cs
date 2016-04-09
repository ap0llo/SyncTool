// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public class NodeComparer<T> : IEqualityComparer<Node<T>>
    {
        readonly IEqualityComparer<T> m_ValueComparer;


        public NodeComparer(IEqualityComparer<T> valueComparer)
        {
            if (valueComparer == null)
            {
                throw new ArgumentNullException(nameof(valueComparer));
            }
            m_ValueComparer = valueComparer;
        }


        public bool Equals(Node<T> x, Node<T> y)
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

        public int GetHashCode(Node<T> obj) => m_ValueComparer.GetHashCode(obj.Value);
    }
}