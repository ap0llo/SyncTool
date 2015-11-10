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

        public EmptyFile()
        {
            
        }

        public EmptyFile(string name) : base(name)
        {
        }

        public Stream OpenRead()
        {
            return new MemoryStream(Array.Empty<byte>());
        }

      
    }
}