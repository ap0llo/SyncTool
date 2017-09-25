using System;
using System.IO;
using NodaTime;

namespace SyncTool.FileSystem.Local
{
    internal class LocalFile : FileSystemItem, IReadableFile, ILocalFile
    {
        readonly FileInfo m_FileInfo;

                   
        public Instant LastWriteTime
        {
            get
            {
                m_FileInfo.Refresh();
                return Instant.FromDateTimeUtc(m_FileInfo.LastWriteTimeUtc);
            }
        }

        public long Length => m_FileInfo.Length;

        public string Location => m_FileInfo.FullName;


        public LocalFile(IDirectory parent, string path) : this(parent, new FileInfo(path))
        {
            
        }

        public LocalFile(IDirectory parent, FileInfo fileInfo) : base(parent, fileInfo.Name)
        {
            m_FileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }


        public Stream OpenRead() => m_FileInfo.OpenRead();

        public IFile WithParent(IDirectory newParent)
        {
            return new LocalFile(newParent, this.m_FileInfo);
        }
    }
}