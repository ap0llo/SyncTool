namespace SyncTool.FileSystem
{
    public static class LocalDirectoryExtensions
    {
        
        public static TemporaryLocalDirectory ToTemporaryDirectory(this ILocalDirectory directory) => new TemporaryLocalDirectory(directory);
    }
}