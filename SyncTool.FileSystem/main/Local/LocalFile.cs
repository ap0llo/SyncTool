using System;
using System.IO;

namespace SyncTool.FileSystem.Local
{
    public class LocalFile : IReadableFile, ILocalFile
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

        public string Location => m_FileInfo.FullName;


        public LocalFile(string path) : this(new FileInfo(path))
        {
            
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