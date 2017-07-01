using System;
using System.Collections.Generic;
using SyncTool.Common.Services;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryService : IItemService<string, IFileSystemHistory>
    {
        /// <summary>
        /// Creates a new filesystem history with the specified name
        /// </summary>
        /// <param name="name">The name of the history to create</param>
        /// <exception cref="DuplicateFileSystemHistoryException">Thrown if a file system history with the specified name already exists</exception>
        void CreateHistory(string name);
        
    }
}