// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem
{
    public class File : FileSystemItem, IFile
    {
        public DateTime LastWriteTime { get; set; }

        public long Length { get; set; }

        public virtual IFile WithParent(IDirectory newParent)
        {
            return new File(newParent, this.Name) { LastWriteTime =  this.LastWriteTime, Length =  this.Length};
        }


        public File(IDirectory parent , string name) : base(parent, name)
        {
        }

    }
}