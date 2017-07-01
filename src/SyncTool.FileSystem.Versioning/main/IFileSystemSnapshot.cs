using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IFileSystemSnapshot
    {
        /// <summary>
        /// Gets the <see cref="IFileSystemHistory"/> this snapshot is part of
        /// </summary>
        IFileSystemHistory History { get; }

        /// <summary>
        /// The id uniquely identifying the snapshot
        /// </summary>
        string Id { get; }

        /// <summary>
        /// The time the snapshot was created
        /// </summary>
        DateTime CreationTime { get; } 

        /// <summary>
        /// The state of the file system stored in the snapshot
        /// </summary>
        IDirectory RootDirectory { get; }
    }
}