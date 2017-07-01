namespace SyncTool.Common.Groups
{
    /// <summary>
    /// Provides paths to local directories that can be used for storage by <see cref="IGroup"/> implementations
    /// </summary>
    interface IGroupDirectoryPathProvider 
    {              
        string GetGroupDirectoryPath(string groupName);
    }
}