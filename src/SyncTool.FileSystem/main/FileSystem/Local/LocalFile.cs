using System;
using System.IO;

namespace SyncTool.FileSystem.Local
{
    internal class LocalFile : FileSystemItem, IReadableFile, ILocalFile
    {
        readonly FileInfo m_FileInfo;

                   
        public DateTime LastWriteTime
        {
            get
            {
                m_FileInfo.Refresh();
                return m_FileInfo.LastWriteTime;
            }
        }

        public long Length => m_FileInfo.Length;

        public string Location => m_FileInfo.FullName;


        public LocalFile(IDirectory parent, string path) : this(parent, new FileInfo(path))
        {
            
        }

        public LocalFile(IDirectory parent, FileInfo fileInfo) : base(parent, fileInfo.Name)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }
            m_FileInfo = fileInfo;
        }


        public Stream OpenRead() => m_FileInfo.OpenRead();

        public IFile WithParent(IDirectory newParent)
        {
            return new LocalFile(newParent, this.m_FileInfo);
        }
    }
}