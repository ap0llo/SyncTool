using System.Collections.Generic;

namespace SyncTool.Synchronization.ChangeGraph
{
    public interface IGraph<T>
    {
        IEnumerable<ValueNode<T>> ValueNodes { get; }
    }
}