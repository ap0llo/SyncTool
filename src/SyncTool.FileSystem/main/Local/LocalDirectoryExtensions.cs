namespace SyncTool.FileSystem.Local
{
    public static class LocalDirectoryExtensions
    {
        /// <summary>
        /// Wraps the local directory into a disposable wrapper
        /// that deletes the directory when it is disposed
        /// </summary>
        public static DisposableLocalDirectoryWrapper ToTemporaryDirectory(this ILocalDirectory directory) => new DisposableLocalDirectoryWrapper(directory);
    }
}