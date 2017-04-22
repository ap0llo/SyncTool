using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public class Directory : InMemoryDirectory, IEnumerable<IFileSystemItem>
    {
        public Directory(string name) : this(null, name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
        }
        public Directory(IDirectory parent, string name) : this(parent, name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
        }

        public Directory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : this(null, name, directories, files)
        {
        }

        public Directory(IDirectory parent, string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : base(parent, name, directories, files)
        {
        }

        public Directory(string name, IEnumerable<IFile> files) : this(null, name, Enumerable.Empty<IDirectory>(), files)
        {
        }

        public Directory(IDirectory parent, string name, IEnumerable<IFile> files) : this(parent, name, Enumerable.Empty<IDirectory>(), files)
        {
        }

        public Directory(string name, IEnumerable<IDirectory> directories) : this(null, name, directories, Enumerable.Empty<IFile>())
        {
        }

        public Directory(IDirectory parent, string name, IEnumerable<IDirectory> directories) : this(parent, name, directories, Enumerable.Empty<IFile>())
        {
        }


        public IEnumerator<IFileSystemItem> GetEnumerator()
        {
            return Directories.Cast<IFileSystemItem>().Union(Files).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        // Make Add() method public
        new public IDirectory Add(Func<IDirectory, IDirectory> createDirectory) => base.Add(createDirectory);
        
        // Make Add() method public
        new public IFile Add(Func<IDirectory, IFile> createFile) => base.Add(createFile);
        
    }
}