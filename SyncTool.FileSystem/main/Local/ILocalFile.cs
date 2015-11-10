// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.FileSystem.Local
{
    public interface ILocalFile : IFile
    {
        /// <summary>
        /// The full path of the file in the local file system
        /// </summary>
         string Location { get; }
    }
}