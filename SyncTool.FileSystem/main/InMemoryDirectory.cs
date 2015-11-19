// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public abstract class InMemoryDirectory : AbstractDirectory
    {
        readonly IDictionary<string, IDirectory> m_Directories;
        readonly IDictionary<string, IFile> m_Files;
        

        public override IEnumerable<IDirectory> Directories => m_Directories.Values;

        public override IEnumerable<IFile> Files => m_Files.Values;
        


        protected InMemoryDirectory(string name, IEnumerable<IDirectory> directories, IEnumerable<IFile> files) : base(name)
        {            
            m_Directories = directories.ToDictionary(dir => dir.Name, StringComparer.InvariantCultureIgnoreCase);
            m_Files = files.ToDictionary(file => file.Name, StringComparer.InvariantCultureIgnoreCase);
        }



        protected override bool FileExistsByName(string name) => m_Files.ContainsKey(name);

        protected override bool DirectoryExistsByName(string name) => m_Directories.ContainsKey(name);

        protected override IFile GetFileByName(string name) => m_Files[name];

        protected override IDirectory GetDirectoryByName(string name) => m_Directories[name];
    
        protected void Add(IDirectory directory) => m_Directories.Add(directory.Name, directory);

        protected void Add(IFile file) => m_Files.Add(file.Name, file);
    }
}