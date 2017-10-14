using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public interface IDirectory : IFileSystemItem
    {        
        IEnumerable<IDirectory> Directories { get; } 

        IEnumerable<IFile> Files { get; }


        IFileSystemItem this[string name] { get; }

        IDirectory GetDirectory(string path);

        IFile GetFile(string path);

        IFile GetFile(FileReference reference);

        bool FileExists(string path);

        bool FileExists(FileReference reference);

        bool DirectoryExists(string path);
    }
}