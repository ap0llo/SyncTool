// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem
{
    public class File : IFile
    {
        
        public string Name { get; set; }

        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }        

        public File()
        {
            
        }

        public File(string name)
        {
            this.Name = name;
        }

    }
}