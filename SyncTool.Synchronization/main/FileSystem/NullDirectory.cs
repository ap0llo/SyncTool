// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    class NullDirectory : IDirectory
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

        public bool FileExists(string path) => false;

        public bool DirectoryExists(string path) => false;
        
    }
}