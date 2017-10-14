using JetBrains.Annotations;
using SyncTool.FileSystem;
using System;
using System.Collections.Generic;

namespace SyncTool.Synchronization.State
{
    //TODO: Replace by sealed class?
    //TODO: Implementors should override ToString()
    public interface ISyncConflict : IEquatable<ISyncConflict>
    {
        string SnapshotId { get; }

        string Path { get; }

        IReadOnlyList<IFileReference> ConflictingVersions { get; }
    }
}
