using System;

namespace SyncTool.FileSystem.Versioning
{
    [Serializable]
    public class DuplicateFileSystemHistoryException : Exception
    {

        public DuplicateFileSystemHistoryException(string name) : base($"A history named '{name}' already exists")
        {
            
        }

    }
}