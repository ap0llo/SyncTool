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

        public IEnumerable<IDirectory> Directories => m_Directories.Values;

        public IEnumerable<IFile> Files => m_Files.Values;

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

        public IDirectory GetDirectory(string name) => m_Directories[name];

        public IFile GetFile(string name) => m_Files[name];

        public bool FileExists(string name) => m_Files.ContainsKey(name);

        public bool DirectoryExists(string name) => m_Directories.ContainsKey(name);
    }
}