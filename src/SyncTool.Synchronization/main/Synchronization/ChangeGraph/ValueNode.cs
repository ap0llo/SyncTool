using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public sealed class ValueNode<T> : Node<T>
    {
        public T Value { get; }


        public ValueNode(T value, IEqualityComparer<T> valueComparer, int index) : base(valueComparer, index)
        {            
            Value = value;                        
        }        
    }
}