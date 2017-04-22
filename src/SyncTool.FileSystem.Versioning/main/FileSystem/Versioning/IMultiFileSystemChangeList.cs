using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IMultiFileSystemChangeList
    {
        string Path { get; }

        IEnumerable<string> HistoryNames { get; }

        IEnumerable<IChange> AllChanges { get; }

        IEnumerable<IChange> GetChanges(string historyName);
    }
}