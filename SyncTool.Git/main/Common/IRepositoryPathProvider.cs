// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace SyncTool.Git.Common
{

    public interface IRepositoryPathProvider 
    {        
        IEnumerable<string> RepositoryPaths { get; }
        
        string GetRepositoryPath(string repositoryName);

    }
}