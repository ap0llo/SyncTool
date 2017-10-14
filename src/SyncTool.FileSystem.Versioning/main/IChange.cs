using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IChange : IEquatable<IChange>
    {
        /// <summary>
        /// The path of the file that was changed
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The type of the change
        /// </summary>
        ChangeType Type { get; }

        /// <summary>
        /// A reference to the file before the modification
        /// </summary>
        FileReference FromVersion { get; }

        /// <summary>
        /// A reference to the file after the modification
        /// </summary>
        FileReference ToVersion { get; }
    }
}