// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Configuration.Model
{
    public interface ISyncGroup
    {
        string Name { get; } 

        IEnumerable<SyncFolder> Folders { get; }

        SyncFolder this[string name] { get; }


        void AddSyncFolder(SyncFolder folder);
         
        IFileSystemHistory GetHistory(string syncFolderName);

    }
}