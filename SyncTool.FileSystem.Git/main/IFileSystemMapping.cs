namespace SyncTool.FileSystem.Git
{
    public interface IFileSystemMapping
    {
        IFile GetMappedFile(IFile originalFile);

        IFile GetOriginalFile(IFile mappedFile);                

        IDirectory GetMappedDirectory(IDirectory originalDirectory);

        IDirectory GetOriginalDirectory(IDirectory mappedDirectory);
    }
}