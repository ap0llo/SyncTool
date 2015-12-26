// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.IO;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration.Reader
{
    public interface ISyncFolderReader
    {
        SyncFolder ReadSyncFolder(Stream stream);
    }
}