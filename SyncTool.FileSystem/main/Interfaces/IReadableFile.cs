using System.IO;

namespace SyncTool.FileSystem
{
    public interface IReadableFile : IFile
    {

        Stream Open(FileMode mode);
        

    }
}