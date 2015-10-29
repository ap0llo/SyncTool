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

        public Stream Open(FileMode mode)
        {
            if (mode != FileMode.Open)
            {
                throw new NotSupportedException();
            }

            return m_Blob.GetContentStream();
        }
    }
}