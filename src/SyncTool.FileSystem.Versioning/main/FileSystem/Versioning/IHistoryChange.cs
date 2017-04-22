using System;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryChange : IEquatable<IHistoryChange>
    {
        string HistoryName { get; } 

        ChangeType Type { get; }
    }
}