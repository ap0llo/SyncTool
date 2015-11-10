// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SyncTool.FileSystem.Local
{
    public class LocalDirectory : ILocalDirectory
    {
        readonly IDictionary<string, IDirectory> m_Directories = new Dictionary<string, IDirectory>(StringComparer.InvariantCultureIgnoreCase);
        readonly IDictionary<string, IFile> m_Files = new Dictionary<string, IFile>(StringComparer.InvariantCultureIgnoreCase);
        readonly DirectoryInfo m_DirectoryInfo;



        public string Name => m_DirectoryInfo.Name;

        public string Location => m_DirectoryInfo.FullName;

        public IEnumerable<IDirectory> Directories
        {
            get
            {
                RefreshDirectories();
                return m_Directories.Values;
            }
        }

        public IEnumerable<IFile> Files
        {
            get
            {
                RefreshFiles();
                return m_Files.Values;
            }
        }

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



        public LocalDirectory(string path) : this(new DirectoryInfo(path))
        {
        }

        public LocalDirectory(DirectoryInfo directoryInfo)
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }
            m_DirectoryInfo = directoryInfo;
        }


        public IDirectory GetDirectory(string name)
        {
            RefreshDirectories();
            return m_Directories[name];
        }

        public IFile GetFile(string name)
        {
            RefreshFiles();
            return m_Files[name];
        }

        public bool FileExists(string name) => System.IO.File.Exists(Path.Combine(m_DirectoryInfo.FullName, name));

        public bool DirectoryExists(string name) => System.IO.Directory.Exists(Path.Combine(m_DirectoryInfo.FullName, name));


        void RefreshDirectories()
        {
            m_DirectoryInfo.Refresh();
            UpdateValueCache(m_DirectoryInfo.GetDirectories(), m_Directories, info => info.Name, dirInfo => new LocalDirectory(dirInfo));
        }

        void RefreshFiles()
        {
            m_DirectoryInfo.Refresh();
            UpdateValueCache(m_DirectoryInfo.GetFiles(), m_Files, fileInfo => fileInfo.Name, fileInfo => new LocalFile(fileInfo));
        }


        static void UpdateValueCache<TValue, TMappedValue>(IEnumerable<TValue> values, IDictionary<string, TMappedValue> mappedValues,
            Func<TValue, string> keySelector, Func<TValue, TMappedValue> mapper)
        {
            var valuesDict = values.ToDictionary(keySelector, StringComparer.InvariantCultureIgnoreCase);

            foreach (var name in mappedValues.Keys.Where(n => !valuesDict.ContainsKey(n)).ToList())
            {
                mappedValues.Remove(name);
            }

            foreach (var name in valuesDict.Keys.Where(n => !mappedValues.ContainsKey(n)))
            {
                mappedValues.Add(name, mapper(valuesDict[name]));
            }
        }

    }
}