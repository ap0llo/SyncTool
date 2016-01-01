// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.IO;
using SyncTool.Common.Utilities;

namespace SyncTool.FileSystem.Local
{
    /// <summary>
    /// Wraps any instance of <see cref="ILocalDirectory"/> and deletes it when the wrapper is disposed
    /// </summary>    
    public sealed class DisposableLocalDirectoryWrapper : IDisposable
    {

        public ILocalDirectory Directory { get; }

        public string Location => Directory.Location;



        public DisposableLocalDirectoryWrapper(ILocalDirectory inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }
            Directory = inner;
        }



        public void Dispose()
        {
            DirectoryHelper.DeleteRecursively(Directory.Location);
        }

    
    }
}