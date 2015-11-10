// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem
{
    public interface IFile : IFileSystemItem
    {
        /// <summary>
        /// The time the file was last modified
        /// </summary>
        DateTime LastWriteTime { get; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        long Length { get; }
        

    }
}