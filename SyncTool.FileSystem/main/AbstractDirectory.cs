// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SyncTool.FileSystem
{
    public abstract class AbstractDirectory : IDirectory
    {

        public virtual string Name { get; }

        public abstract IEnumerable<IDirectory> Directories { get; }

        public abstract IEnumerable<IFile> Files { get; }

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


        protected AbstractDirectory(string name)
        {
            Name = name;            
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
                return FileExistsByName(localName);
            }
            else
            {
                return GetDirectoryByName(localName).FileExists(remainingPath);
            }
        }

        protected abstract bool FileExistsByName(string name);


        public virtual bool DirectoryExists(string path)
        {
            EnsurePathIsValid(path);

            string localName;
            string remainingPath;
            ParsePath(path, out localName, out remainingPath);

            if (remainingPath == "")
            {
                return DirectoryExistsByName(localName);                
            }
            else
            {
                return GetDirectoryByName(localName).DirectoryExists(remainingPath);
            }
        }

        protected abstract bool DirectoryExistsByName(string name);


        protected abstract IFile GetFileByName(string name);

        protected abstract IDirectory GetDirectoryByName(string name);
        

        private void EnsurePathIsValid(string path)
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

            if (Constants.InvalidPathCharacters.Any(path.Contains))
            {
                throw new FormatException("The path contains invalid characters");
            }
                       
        }

        private void ParsePath(string path, out string localName, out string remainingPath)
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

        
        
    }
}