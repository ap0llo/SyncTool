// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public abstract class AbstractDirectory : IDirectory
    {
        protected readonly IDictionary<string, IDirectory> m_Directories;
        protected readonly IDictionary<string, IFile> m_Files;

        public string Name { get; }

        public virtual IEnumerable<IDirectory> Directories => m_Directories.Values;

        public virtual IEnumerable<IFile> Files => m_Files.Values;

        public IFileSystemItem this[string name]
        {
            get
            {
                if (FileExists(name))
                {
                    return GetFile(name);
                }
                return GetDirectory(name);
            }
        }


        protected AbstractDirectory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files)
        {
            Name = name;
            m_Directories = directories.ToDictionary(dir => dir.Name, StringComparer.InvariantCultureIgnoreCase);
            m_Files = files.ToDictionary(file => file.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        public virtual IDirectory GetDirectory(string name) => m_Directories[name];

        public virtual IFile GetFile(string name) => m_Files[name];

        public virtual bool FileExists(string name) => m_Files.ContainsKey(name);

        public virtual bool DirectoryExists(string name) => m_Directories.ContainsKey(name);
    }
}