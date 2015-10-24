using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IFileSystemDiff
    {
         
        IFileSystemSnapshot LeftState { get; }

        IFileSystemSnapshot RightState { get; }


        IEnumerable<IChange> Changes { get; } 

    }
}