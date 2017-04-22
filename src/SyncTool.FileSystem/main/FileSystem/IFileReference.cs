using System;

namespace SyncTool.FileSystem
{
    public interface IFileReference : IEquatable<IFileReference>
    {
        /// <summary>
        /// The path of the referenced file
        /// </summary>
        string Path { get; }

        /// <summary>
        /// The LastWriteTime of the referenced file (or null, if no specific write time is being referenced)
        /// </summary>
        DateTime? LastWriteTime { get; }

        /// <summary>
        /// The size of the referenced file in bytes (or null, if no specific size is being referenced)
        /// </summary>
        long? Length { get; }
    }
}