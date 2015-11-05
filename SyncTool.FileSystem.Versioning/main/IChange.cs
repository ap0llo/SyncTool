using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IChange
    {
        ChangeType Type { get; } 

        IFile File { get; }
    }
}