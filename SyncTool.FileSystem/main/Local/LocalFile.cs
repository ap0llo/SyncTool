// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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

        public Stream OpenRead() => m_FileInfo.OpenRead();

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