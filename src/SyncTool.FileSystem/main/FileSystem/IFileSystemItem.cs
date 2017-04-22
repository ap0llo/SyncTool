namespace SyncTool.FileSystem
{
    public interface IFileSystemItem
    {
        string Name { get; } 

        string Path { get; }

        IDirectory Parent { get; }
    }
}