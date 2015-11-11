// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public class Directory : AbstractDirectory, IEnumerable<IFileSystemItem>
    {
        public Directory(string name) : this(name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
        }

        public Directory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : base(name, directories, files)
        {
        }

        public Directory(string name, IEnumerable<IFile> files) : this(name, Enumerable.Empty<IDirectory>(), files)
        {
        }

        public Directory(string name, IEnumerable<IDirectory> directories) : this(name, directories, Enumerable.Empty<IFile>())
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


        new public void Add(IDirectory directory)
        {
            base.Add(directory);
        }

        new public void Add(IFile file)
        {
            base.Add(file);
        }
    }
}