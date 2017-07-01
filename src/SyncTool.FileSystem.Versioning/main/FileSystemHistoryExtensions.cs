using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Extension methods for <see cref="IFileSystemHistory"/>
    /// </summary>
    public static class FileSystemHistoryExtensions
    {
        /// <summary>
        /// Gets the oldest snapshot from the history
        /// </summary>        
        public static IFileSystemSnapshot GetOldestSnapshot(this IFileSystemHistory history) 
            => history.Snapshots.OrderBy(x => x.CreationTime).First();
    }
}