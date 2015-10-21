using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public class Directory : FileSystemItem
    {

        readonly IDictionary<string, Directory> m_Directories = new Dictionary<string, Directory>(StringComparer.InvariantCultureIgnoreCase); 
        readonly IDictionary<string, File> m_Files = new Dictionary<string, File>(StringComparer.InvariantCultureIgnoreCase); 

        public IEnumerable<Directory> Directories => m_Directories.Values;

        public IEnumerable<File> Files => m_Files.Values;


        public FileSystemItem this[string name]
        {
            get
            {
                File file;
                if (m_Files.TryGetValue(name, out file))
                {
                    return file;
                }
                else
                {
                    return m_Directories[name];
                }
            }
        }


        public override bool Equals(object obj) => Equals(obj as Directory);

        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Path);
        
        public bool Equals(Directory other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Path, other.Path);
        }


        public File Add(File file)
        {
            file.Parent = this;
            m_Files.Add(file.Name, file);
            return file;
        }

        public Directory Add(Directory directory)
        {
            directory.Parent = this;
            m_Directories.Add(directory.Name, directory);
            return directory;
        }
    }
}