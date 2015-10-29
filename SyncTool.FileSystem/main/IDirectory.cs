using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace SyncTool.FileSystem
{
    public interface IDirectory : IFileSystemItem
    {        
        IEnumerable<IDirectory> Directories { get; } 

        IEnumerable<IFile> Files { get; }


        IFileSystemItem this[string name] { get; }

        IDirectory GetDirectory(string name);

        IFile GetFile(string name);

        bool FileExists(string name);

        bool DirectoryExists(string name);

    }
}