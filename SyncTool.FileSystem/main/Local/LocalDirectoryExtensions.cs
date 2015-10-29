namespace SyncTool.FileSystem.Local
{
    public static class LocalDirectoryExtensions
    {
        
        public static TemporaryLocalDirectory ToTemporaryDirectory(this ILocalDirectory directory) => new TemporaryLocalDirectory(directory);
    }
}