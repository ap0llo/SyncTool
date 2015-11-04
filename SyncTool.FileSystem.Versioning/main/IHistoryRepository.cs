
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    public interface IHistoryRepository
    {
                 
        IEnumerable<IFileSystemHistory> Histories { get; }

        IFileSystemHistory CreateHistory(string name);

    }
}