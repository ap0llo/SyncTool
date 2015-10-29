using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public class IChange
    {
        ChangeType Type { get; } 

        IFile File { get; }
    }
}