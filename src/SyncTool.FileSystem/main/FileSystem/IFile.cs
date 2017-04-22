using System;

namespace SyncTool.FileSystem
{
    public interface IFile : IFileSystemItem
    {
        /// <summary>
        /// The time the file was last modified
        /// </summary>
        DateTime LastWriteTime { get; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        long Length { get; }
        


        IFile WithParent(IDirectory newParent);

    }
}