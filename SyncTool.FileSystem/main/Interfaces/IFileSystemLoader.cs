namespace SyncTool.FileSystem
{
    public interface IFileSystemLoader
    {


        IFileSystemSnapshot LoadFileSystemState();

    }
}