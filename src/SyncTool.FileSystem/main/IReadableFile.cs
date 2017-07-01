using System.IO;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Extension of the <see cref="IFile"/> interface that also supports reading the content of the file
    /// </summary>
    public interface IReadableFile : IFile
    {
        Stream OpenRead();
    }
}