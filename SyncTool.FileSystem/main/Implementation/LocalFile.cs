using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public class LocalFile : IReadableFile
    {
        readonly FileInfo m_FileInfo;

        
        public string Name
        {
            get
            {
                m_FileInfo.Refresh();
                return m_FileInfo.Name;
            }
        }

        public DateTime LastWriteTime
        {
            get
            {
                m_FileInfo.Refresh();
                return m_FileInfo.LastWriteTime;
            }
        }

        public long Length => m_FileInfo.Length;

        public Stream Open(FileMode mode) => m_FileInfo.Open(mode);
        

        public void Accept<T>(IFileSystemVisitor<T> visitor, T parameter)
        {
            visitor.Visit(this, parameter);
        }


        public LocalFile(FileInfo fileInfo)
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }
            m_FileInfo = fileInfo;
        }

    }
}