
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    public interface IHistoryRepository
    {
                 
        IEnumerable<IFileSystemHistory> Histories { get; }

        IFileSystemHistory CreateHistory(string name);

    }
}