// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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

        IDirectory GetDirectory(string path);

        IFile GetFile(string path);

        bool FileExists(string path);

        bool DirectoryExists(string path);

    }
}