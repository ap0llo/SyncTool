using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Implementation of <see cref="IDirectory"/> that has a fixed Path and Name
    /// This allows files to be copied without their tree while still retaining the functionality of the Path property
    /// </summary>
    public class NullDirectory : IDirectory
    {
        public string Name { get; }

        public string Path { get;  }

        public IDirectory Parent => null;

        public IEnumerable<IDirectory> Directories { get { throw new NotSupportedException(); } }

        public IEnumerable<IFile> Files { get { throw new NotSupportedException(); } }

        public IFileSystemItem this[string name]
        {
            get { throw new NotSupportedException(); }
        }


        public NullDirectory(IDirectory directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }
            this.Name = directory.Name;
            this.Path = directory.Path;
        }

        public NullDirectory(string path, string name)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            this.Path = path;
            this.Name = name;
        }


        public IDirectory GetDirectory(string path)
        {
            throw new NotSupportedException();
        }

        public IFile GetFile(string path)
        {
            throw new NotSupportedException();
        }

        public IFile GetFile(IFileReference reference)
        {
            throw new NotSupportedException();
        }

        public bool FileExists(string path) => false;

        public bool FileExists(IFileReference reference) => false;

        public bool DirectoryExists(string path) => false;
        
    }
}