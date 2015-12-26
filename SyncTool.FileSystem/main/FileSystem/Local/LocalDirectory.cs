// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyncTool.Common.Utilities;

namespace SyncTool.FileSystem.Local
{
    public class LocalDirectory : AbstractDirectory, ILocalDirectory, IDisposable
    {
        readonly DirectoryInfo m_DirectoryInfo;
        readonly CachingObjectMapper<DirectoryInfo, LocalDirectory> m_DirectoryMapper;
        readonly CachingObjectMapper<FileInfo, LocalFile> m_FileMapper;



        public string Location => m_DirectoryInfo.FullName;

        public override IEnumerable<IDirectory> Directories
        {
            get
            {
                m_DirectoryInfo.Refresh();
                var directories = m_DirectoryInfo.GetDirectories();
                m_DirectoryMapper.CleanCache(directories);
                return directories.Select(m_DirectoryMapper.MapObject);
            }
        }

        public override IEnumerable<IFile> Files
        {
            get
            {
                m_DirectoryInfo.Refresh();
                var files = m_DirectoryInfo.GetFiles();
                m_FileMapper.CleanCache(files);
                return files.Select(m_FileMapper.MapObject);
            }
        }

        protected override bool FileExistsByName(string name)
        {
            return System.IO.File.Exists(System.IO.Path.Combine(m_DirectoryInfo.FullName, name));
        }

        protected override bool DirectoryExistsByName(string name)
        {
            return System.IO.Directory.Exists(System.IO.Path.Combine(m_DirectoryInfo.FullName, name));
        }

        protected override IFile GetFileByName(string name)
        {
            m_DirectoryInfo.Refresh();
            var fileInfo = m_DirectoryInfo.GetFiles().Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return m_FileMapper.MapObject(fileInfo);
        }

        protected override IDirectory GetDirectoryByName(string name)
        {
            m_DirectoryInfo.Refresh();
            var dirInfo = m_DirectoryInfo.GetDirectories().Single(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return m_DirectoryMapper.MapObject(dirInfo);
        }


        public LocalDirectory(IDirectory parent, string path) : this(parent, new DirectoryInfo(path))
        {
        }

        public LocalDirectory(IDirectory parent, DirectoryInfo directoryInfo) : base(parent, directoryInfo.Name)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }
            m_DirectoryInfo = directoryInfo;

            m_DirectoryMapper = new CachingObjectMapper<DirectoryInfo, LocalDirectory>(dirInfo => new LocalDirectory(this, dirInfo), new NameOnlyFileSystemInfoEqualityComparer());
            m_FileMapper = new CachingObjectMapper<FileInfo, LocalFile>(fileInfo => new LocalFile(this, fileInfo), new NameOnlyFileSystemInfoEqualityComparer());
        }



      


        private class  NameOnlyFileSystemInfoEqualityComparer : IEqualityComparer<FileSystemInfo>
        {
            public bool Equals(FileSystemInfo x, FileSystemInfo y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return StringComparer.InvariantCultureIgnoreCase.Equals(x.Name, y.Name);
            }

            public int GetHashCode(FileSystemInfo obj)
            {
                return obj == null ? 0 : StringComparer.InvariantCultureIgnoreCase.GetHashCode(obj.Name);
            }
        }

        public void Dispose()
        {
            m_DirectoryMapper.Dispose();
            m_FileMapper.Dispose();
        }
    }
}