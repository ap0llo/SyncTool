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

        public virtual string Name { get; }

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


        public virtual IDirectory GetDirectory(string path)
        {
            EnsurePathIsValid(path);
            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return GetDirectoryByName(localName);
            }
            else
            {
                return GetDirectoryByName(localName).GetDirectory(remainingPath);
            }
        }

        public virtual IFile GetFile(string path)
        {
            EnsurePathIsValid(path);

            string localName;
            string remainingPath;            
            ParsePath(path, out localName, out remainingPath);       
            
            if(remainingPath == "")
            {
                return GetFileByName(localName);
            }
            else
            {
                return GetDirectoryByName(localName).GetFile(remainingPath);
            }
        }

        public virtual bool FileExists(string path)
        {
            EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return m_Files.ContainsKey(localName);
            }
            else
            {
                return GetDirectoryByName(localName).FileExists(remainingPath);
            }
        }

        public virtual bool DirectoryExists(string path)
        {
            EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return m_Directories.ContainsKey(path);                
            }
            else
            {
                return GetDirectoryByName(localName).DirectoryExists(remainingPath);
            }
        }



        protected T GetItemByPath<T>(string path, Func<IDirectory, string, T> getByNameAction) where T : IFileSystemItem
        {
            if (!path.Contains(Constants.DirectorySeparatorChar))
            {
                return getByNameAction.Invoke(this, path);
            }
            else
            {
                var splitIndex = path.IndexOf(Constants.DirectorySeparatorChar);
                var name = path.Substring(0, splitIndex);
                var remainingPath = path.Substring(splitIndex + 1);

                var directory = GetDirectoryByName(name);
                return (T)getByNameAction.Invoke(directory, remainingPath);
            }            
        }

        protected IFile GetFileByName(string name)
        {
            return m_Files[name];
        }

        protected IDirectory GetDirectoryByName(string name)
        {
            return m_Directories[name];
        }

        protected void EnsurePathIsValid(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (String.IsNullOrWhiteSpace(path))
            {
                throw new FormatException($"'{nameof(path)}' must not be null or empty");
            }

            if (path[0] == Constants.DirectorySeparatorChar)
            {
                throw new FormatException($"'{nameof(path)}' must not start with '{Constants.DirectorySeparatorChar}'");
            }

            if (path[path.Length - 1] == Constants.DirectorySeparatorChar)
            {
                throw new FormatException($"'{nameof(path)}' must not end with '{Constants.DirectorySeparatorChar}'");
            }               
        }

        protected void ParsePath(string path, out string localName, out string remainingPath)
        {
            if (path.Contains(Constants.DirectorySeparatorChar))
            {
                var splitIndex = path.IndexOf(Constants.DirectorySeparatorChar);
                localName = path.Substring(0, splitIndex);
                remainingPath = path.Substring(splitIndex + 1);
            }
            else
            {
                localName = path;
                remainingPath = "";
            }
        }

        protected void Add(IDirectory directory) => m_Directories.Add(directory.Name, directory);

        protected void Add(IFile file) => m_Files.Add(file.Name, file);

        
    }
}