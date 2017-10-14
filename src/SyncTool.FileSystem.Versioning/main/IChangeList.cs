using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Groups all changes for a file
    /// </summary>
    public interface IChangeList
    {
        string Path { get; }

        IEnumerable<Change> Changes { get; } 
    }
}