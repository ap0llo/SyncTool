// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem
{
    public interface IFileSystemSnapshot
    {
        string Id { get; }

        DateTime CreationTime { get; } 

        IDirectory RootDirectory { get; }
    }
}