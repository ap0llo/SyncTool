using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemChangeList
    {
        string Path { get; }

        IEnumerable<string> HistoryNames { get; }

        IEnumerable<Change> AllChanges { get; }

        IEnumerable<Change> GetChanges(string historyName);
    }
}