// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using SyncTool.FileSystem;

namespace SyncTool.Git.FileSystem
{
    public interface IFileSystemMapping
    {
        IFile GetMappedFile(IFile originalFile);

        IFile GetOriginalFile(IFile mappedFile);

        IDirectory GetMappedDirectory(IDirectory originalDirectory);

        IDirectory GetOriginalDirectory(IDirectory mappedDirectory);
    }
}