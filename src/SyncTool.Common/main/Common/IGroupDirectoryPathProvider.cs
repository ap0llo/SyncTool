using System.Collections.Generic;

namespace SyncTool.Common
{

    /// <summary>
    /// Provides paths to local directories that can be used for storage by <see cref="IGroup"/> implementations
    /// </summary>
    //TODO: Make internal
    public interface IGroupDirectoryPathProvider 
    {              
        string GetGroupDirectoryPath(string groupName);

    }
}