using JetBrains.Annotations;
using System;

namespace SyncTool.Synchronization.State
{
    //TODO: Replace by sealed class?
    //TODO: Implementors should override ToString()
    public interface IConflict : IEquatable<IConflict>
    {
        [NotNull]
        string SnapshotId { get; }

        string ToString();
    }
}
