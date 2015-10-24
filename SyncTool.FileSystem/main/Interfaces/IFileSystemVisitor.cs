namespace SyncTool.FileSystem
{
    public interface IFileSystemVisitor<T>
    {
        
        void Visit(IFile file, T parameter);

        void Visit(IReadableFile file, T parameter);

        void Visit(IDirectory directory, T parameter);
    }
}