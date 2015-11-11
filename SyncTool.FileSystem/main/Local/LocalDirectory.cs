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
    public class LocalDirectory : AbstractDirectory, ILocalDirectory
    {
        readonly DirectoryInfo m_DirectoryInfo;
             

        public string Location => m_DirectoryInfo.FullName;

        public override IEnumerable<IDirectory> Directories
        {
            get
            {
                RefreshDirectories();
                return base.Directories;
            }
        }

        public override IEnumerable<IFile> Files
        {
            get
            {
                RefreshFiles();
                return base.Files;                
            }
        }


        public LocalDirectory(string path) : this(new DirectoryInfo(path))
        {
        }

        public LocalDirectory(DirectoryInfo directoryInfo) : base(directoryInfo.Name, Enumerable.Empty<IDirectory>(), Enumerable.Empty<IFile>())
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException(nameof(directoryInfo));
            }
            m_DirectoryInfo = directoryInfo;
        }


        public override IDirectory GetDirectory(string path)
        {
            RefreshDirectories();
            return base.GetDirectory(path);
        }

        public override IFile GetFile(string path)
        {
            RefreshFiles();
            return base.GetFile(path);
        }

        public override bool FileExists(string path)
        {
            RefreshFiles();
            return base.FileExists(path);
        }

        public override bool DirectoryExists(string path)
        {
            RefreshDirectories();
            return base.DirectoryExists(path);
        }


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