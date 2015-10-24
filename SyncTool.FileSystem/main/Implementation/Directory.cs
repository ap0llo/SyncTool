using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public class Directory : IDirectory, IEnumerable<IFileSystemItem>
    {
        readonly IDictionary<string, IDirectory> m_Directories;
        readonly IDictionary<string, IFile> m_Files;
        

        public string Name { get; set; }

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
                else
                {
                    return GetDirectory(name);                    
                }
            }
        }

        public IDirectory GetDirectory(string name) => m_Directories[name];

        public IFile GetFile(string name) => m_Files[name];
        
        public bool FileExists(string name) => m_Files.ContainsKey(name);

        public bool DirectoryExists(string name) => m_Directories.ContainsKey(name);


        public Directory(string name) : this(name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
            Name = name;
        }

        public Directory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files)
        {
            Name = name;
            m_Directories = directories.ToDictionary(dir => dir.Name, StringComparer.InvariantCultureIgnoreCase);
            m_Files = files.ToDictionary(file => file.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        public Directory(string name, IEnumerable<IFile> files): this(name, Enumerable.Empty<IDirectory>(), files)
        {
            
        }

        public Directory(string name, IEnumerable<IDirectory> directories) : this(name, directories, Enumerable.Empty<IFile>())
        {

        }


        public IEnumerator<IFileSystemItem> GetEnumerator()
        {
            return m_Directories.Values.Cast<IFileSystemItem>().Union(m_Files.Values).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public void Add(IDirectory directory)
        {
            m_Directories.Add(directory.Name, directory);
        }

        public void Add(IFile file)
        {
            m_Files.Add(file.Name, file);
        }
    }
}