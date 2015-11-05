using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IFileSystemDiff
    {
         
        IFileSystemSnapshot FromSnapshot { get; }

        IFileSystemSnapshot ToSnapshot { get; }


        IEnumerable<IChange> Changes { get; } 

    }
}