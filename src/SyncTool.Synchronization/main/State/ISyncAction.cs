using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.State
{
    //TODO: Replace by sealed class?
    //TODO: Implementors should override ToString()
    public interface ISyncAction : IEquatable<ISyncAction>
    {
        [NotNull]
        string SnapshotId { get; }

        /// <summary>
        /// The path of the file that was changed
        /// </summary>
        string Path { get; }

        
        /// <summary>
        /// A reference to the file before the modification
        /// </summary>
        IFileReference FromVersion { get; }

        /// <summary>
        /// A reference to the file after the modification
        /// </summary>
        IFileReference ToVersion { get; }

        string ToString();
    }
}
