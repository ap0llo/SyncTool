using System;

namespace SyncTool.FileSystem
{
    public interface ILocalDirectory : IDirectory
    {

        string Location { get; }

    }
}