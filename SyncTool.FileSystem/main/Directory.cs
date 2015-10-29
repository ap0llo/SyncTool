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