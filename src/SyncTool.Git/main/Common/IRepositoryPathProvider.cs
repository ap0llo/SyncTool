// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;

namespace SyncTool.Git.Common
{

    public interface IRepositoryPathProvider 
    {              
        string GetRepositoryPath(string repositoryName);

    }
}