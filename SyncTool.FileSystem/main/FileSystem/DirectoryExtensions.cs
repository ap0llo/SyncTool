// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem
{
    public static class DirectoryExtensions
    {
        
        public static IEnumerable<IFile> EnumerateFilesRecursively(this IDirectory directory)
        {
            foreach (var file in directory.Files)
            {
                yield return file;

            }

            foreach (var dir in directory.Directories)
            {
                foreach (var file in dir.EnumerateFilesRecursively())
                {
                    yield return file;
                }
            }
        }


    }
}