﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using SyncTool.Configuration.Git.Model;

namespace SyncTool.Configuration.Git.Reader
{
    public interface ISyncRepositoryReader
    {
        IEnumerable<SyncRepositorySettings> GetSyncRepositories();
    }
}