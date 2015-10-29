using System;
using System.Collections.Generic;
using System.IO;

namespace SyncTool.FileSystem.Local
{
    /// <summary>
    /// Wraps any instance of <see cref="ILocalDirectory"/> and deletes it when the wrapper is disposed
    /// </summary>
    public class TemporaryLocalDirectory : ILocalDirectory, IDisposable
    {
        readonly ILocalDirectory m_Inner;


        public string Name => m_Inner.Name;

        public IEnumerable<IDirectory> Directories => m_Inner.Directories;

        public IEnumerable<IFile> Files => m_Inner.Files;

        public IFileSystemItem this[string name] => m_Inner[name];

        public string Location => m_Inner.Location;


        public TemporaryLocalDirectory(ILocalDirectory inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }
            m_Inner = inner;
        }


        public virtual void Dispose()
        {
            var directoryInfo = new DirectoryInfo(m_Inner.Location);
            // remove read-only flag from all files before deleting    
            directoryInfo.Attributes = FileAttributes.Normal;
            foreach (var info in directoryInfo.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directoryInfo.Delete(true);
        }

        public IDirectory GetDirectory(string name) => m_Inner.GetDirectory(name);

        public IFile GetFile(string name) => m_Inner.GetFile(name);

        public bool FileExists(string name) => m_Inner.FileExists(name);

        public bool DirectoryExists(string name) => m_Inner.DirectoryExists(name);
    }
}