
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IHistoryManager
    {
                 
        IEnumerable<IFileSystemHistory> Histories { get; }

        IFileSystemHistory CreateHistory(string name);

    }
}