// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Filtering
{
    //TODO
//    public class FilteringDirectoryDecorator : IDirectory
//    {
//        readonly IDirectory m_WrappedDirectory;
//
//
//        public string Name => m_WrappedDirectory.Name;     
//
//        public IEnumerable<IDirectory> Directories
//        {
//            get
//            {
//                return m_WrappedDirectory.Directories
//                    .Where(dir => IsFiltered(dir) == false)
//                    .Select(dir => new FilteringDirectoryDecorator(dir))
//                    .ToList();
//            }
//        }
//
//        public IEnumerable<IFile> Files => m_WrappedDirectory.Files.Where(file => IsFiltered(file) == false);
//
//        public IFileSystemItem this[string name]
//        {
//            get
//            {
//                var item = m_WrappedDirectory[name];
//
//                //TODO: replace with more appropriate exception             
//                if (IsFiltered(item))
//                {
//                    throw new KeyNotFoundException();
//                }
//
//                return item;
//            }
//        }
//
//
//
//        public FilteringDirectoryDecorator(IDirectory wrappedDirectory)
//        {
//            if (wrappedDirectory == null)
//            {
//                throw new ArgumentNullException(nameof(wrappedDirectory));
//            }
//            m_WrappedDirectory = wrappedDirectory;
//        }
//
//
//        
//        public IDirectory GetDirectory(string path)
//        {
//            var directory = m_WrappedDirectory.GetDirectory(path);
//
//            //TODO: replace with more appropriate exception             
//            if (IsFiltered(directory))
//            {
//                throw new KeyNotFoundException();
//            }
//
//            return new FilteringDirectoryDecorator(directory);
//        }
//
//        public IFile GetFile(string path)
//        {
//            var file = m_WrappedDirectory.GetFile(path);
//
//            //TODO: replace with more appropriate exception             
//            if (IsFiltered(file))
//            {
//                throw new KeyNotFoundException();
//            }
//
//            return file;
//        }
//
//        public bool FileExists(string path)
//        {
//            return m_WrappedDirectory.FileExists(path) && !IsFiltered(GetFile(path));
//        }
//
//        public bool DirectoryExists(string path)
//        {
//            return m_WrappedDirectory.DirectoryExists(path) && !IsFiltered(GetDirectory(path));
//        }
//
//
//
//        protected bool IsFiltered(IFileSystemItem fileSystemItem)
//        {
//            //TODO: Implement filtering
//            return false;
//        }
//        
//
//    }
}