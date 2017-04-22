using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public sealed class StartNode<T> : Node<T>
    {
        public StartNode(IEqualityComparer<T> valueComparer, int index) : base(valueComparer, index)
        {
        }
    }
}