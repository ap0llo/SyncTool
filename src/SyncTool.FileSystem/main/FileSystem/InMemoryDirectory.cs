using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Base class for implementations of <see cref="IDirectory"/> that hold data in memory
    /// </summary>
    public abstract class InMemoryDirectory : AbstractDirectory
    {
        readonly IDictionary<string, IDirectory> m_Directories;
        readonly IDictionary<string, IFile> m_Files;
        

        public override IEnumerable<IDirectory> Directories => m_Directories.Values;

        public override IEnumerable<IFile> Files => m_Files.Values;
        


        protected InMemoryDirectory(IDirectory parent, string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : base(parent, name)
        {            
            m_Directories = directories.ToDictionary(dir => dir.Name, StringComparer.InvariantCultureIgnoreCase);
            m_Files = files.ToDictionary(file => file.Name, StringComparer.InvariantCultureIgnoreCase);
        }



        protected override bool FileExistsByName(string name) => m_Files.ContainsKey(name);

        protected override bool DirectoryExistsByName(string name) => m_Directories.ContainsKey(name);

        protected override IFile GetFileByName(string name) => m_Files[name];

        protected override IDirectory GetDirectoryByName(string name) => m_Directories[name];
    
        /// <summary>
        /// Adds a directory
        /// </summary>
        protected IDirectory Add(Func<IDirectory, IDirectory> createDirectory)
        {
            var directory = createDirectory(this);            
            m_Directories.Add(directory.Name, directory);
            return directory;
        }

        /// <summary>
        /// Adds a file to the directory
        /// </summary>
        protected IFile Add(Func<IDirectory, IFile> createFile)
        {
            var file = createFile(this);
            m_Files.Add(file.Name, file);
            return file;
        }
        
        /// <summary>
        /// Removes the file with the specified name from the directory 
        /// </summary>        
        protected void RemoveFileByName(string name)
        {
            m_Files.Remove(name);
        }
    }
}