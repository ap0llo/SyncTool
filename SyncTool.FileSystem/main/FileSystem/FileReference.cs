// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem
{
    public sealed class FileReference : IFileReference
    {
        public string Path { get; }

        public DateTime? LastWriteTime { get; }

        public long? Length { get; }


        public FileReference(string path, DateTime? lastWriteTime = null, long? length = null)
        {
            PathValidator.EnsurePathIsValid(path);

            Path = path;
            LastWriteTime = lastWriteTime;
            Length = length;
        }
    }
}