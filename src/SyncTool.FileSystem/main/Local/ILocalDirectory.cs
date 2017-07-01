namespace SyncTool.FileSystem.Local
{
    /// <summary>
    /// Interface for a local directory (a <see cref="IDirectory"/> that has a physical location in the file system)
    /// </summary>
    public interface ILocalDirectory : IDirectory
    {
        /// <summary>
        /// Gets the path of the item in the file system
        /// </summary>
        string Location { get; }

    }
}