namespace SyncTool.FileSystem.Local
{
    /// <summary>
    /// Interface for local files (a <see cref="IFile"/> that has a physical location in the file system)
    /// </summary>
    public interface ILocalFile : IFile
    {
        /// <summary>
        /// The full path of the file in the local file system
        /// </summary>
         string Location { get; }
    }
}