// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SyncTool.Utilities;

namespace SyncTool.FileSystem.Local
{
    public class LocalDirectory : AbstractDirectory, ILocalDirectory
    {
        readonly DirectoryInfo m_DirectoryInfo;
        readonly CachingObjectMapper<DirectoryInfo, LocalDirectory> m_DirectoryMapper;
        readonly CachingObjectMapper<FileInfo, LocalFile> m_FileMapper;



        public string Location => m_DirectoryInfo.FullName;

        public override IEnumerable<IDirectory> Directories
        {
            get
            {
                RefreshDirectories();
                return base.Directories;
            }
        }

        public override IEnumerable<IFile> Files
        {
            get
            {
                RefreshFiles();
                return base.Files;                
            }
        }


        public LocalDirectory(string path) : this(new DirectoryInfo(path))
        {
        }

        public LocalDirectory(DirectoryInfo directoryInfo) : base(directoryInfo.Name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }
            m_DirectoryInfo = directoryInfo;

            m_DirectoryMapper = new CachingObjectMapper<DirectoryInfo, LocalDirectory>(dirInfo => new LocalDirectory(dirInfo), new NameOnlyFileSystemInfoEqualityComparer());
            m_FileMapper = new CachingObjectMapper<FileInfo, LocalFile>(fileInfo => new LocalFile(fileInfo), new NameOnlyFileSystemInfoEqualityComparer());
        }


        public override IDirectory GetDirectory(string path)
        {
            RefreshDirectories();
            return base.GetDirectory(path);
        }

        public override IFile GetFile(string path)
        {
            RefreshFiles();
            return base.GetFile(path);
        }

        public override bool FileExists(string path)
        {
            RefreshFiles();
            return base.FileExists(path);
        }

        public override bool DirectoryExists(string path)
        {
            RefreshDirectories();
            return base.DirectoryExists(path);
        }


        void RefreshDirectories()
        {
            m_DirectoryInfo.Refresh();

            m_DirectoryMapper.CleanCache(m_DirectoryInfo.GetDirectories());

            m_Directories.Clear();
            foreach (var dirInfo in m_DirectoryInfo.GetDirectories())
            {
                var dir = m_DirectoryMapper.MapObject(dirInfo);
                m_Directories.Add(dir.Name, dir);
            }
            
        }

        void RefreshFiles()
        {
            m_DirectoryInfo.Refresh();
            var fileInfos = m_DirectoryInfo.GetFiles();

            m_FileMapper.CleanCache(fileInfos);

            m_Files.Clear();

            foreach (var fileInfo in fileInfos)
            {
                var file = m_FileMapper.MapObject(fileInfo);
                m_Files.Add(file.Name, file);
            }            
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

    }
}