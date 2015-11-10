// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class GitFile : IReadableFile
    {
        readonly Blob m_Blob;
        public string Name { get; }

        public DateTime LastWriteTime { get; }

        public long Length => m_Blob.Size;



        public GitFile(string name, DateTime commitTime, Blob blob)
        {
            m_Blob = blob;
            Name = name;
            LastWriteTime = commitTime;
        }

        public Stream OpenRead() => m_Blob.GetContentStream();
    }
}