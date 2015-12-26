// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public class EmptyFile : File, IReadableFile
    {

        public EmptyFile(string name) : this(null, name)
        {
            
        }

        public EmptyFile(IDirectory parent, string name) : base(parent, name)
        {
        }

        public Stream OpenRead()
        {
            return new MemoryStream(Array.Empty<byte>());
        }


        public override IFile WithParent(IDirectory newParent)
        {
            return new EmptyFile(newParent, this.Name) { LastWriteTime =  this.LastWriteTime, Length =  this.Length};
        }
    }
}