// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------


namespace SyncTool.FileSystem.Versioning
{
    //TODO: Find a better name for this class
    public interface IExtendedChange : IChange
    {
         IFileSystemHistory History { get; }
    }
}