using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem;
using NodaTime;

namespace SyncTool.Git.FileSystem
{
    public class GitFile : FileSystemItem, IReadableFile
    {
        readonly Blob m_Blob;
        

        public Instant LastWriteTime { get; }

        public long Length => m_Blob.Size;


        public GitFile(IDirectory parent, string name, Instant commitTime, Blob blob) : base(parent, name)
        {
            m_Blob = blob;            
            LastWriteTime = commitTime;
        }


        public Stream OpenRead() => m_Blob.GetContentStream();

        public IFile WithParent(IDirectory newParent) => new GitFile(newParent, this.Name, this.LastWriteTime, m_Blob);
    }
}