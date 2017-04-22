using System;
using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem
{
    public class GitFile : FileSystemItem, IReadableFile
    {
        readonly Blob m_Blob;
        

        public DateTime LastWriteTime { get; }

        public long Length => m_Blob.Size;


        public GitFile(IDirectory parent, string name, DateTime commitTime, Blob blob) : base(parent, name)
        {
            m_Blob = blob;            
            LastWriteTime = commitTime;
        }


        public Stream OpenRead() => m_Blob.GetContentStream();

        public IFile WithParent(IDirectory newParent)
        {
            return new GitFile(newParent, this.Name, this.LastWriteTime, m_Blob);
        }

    }
}