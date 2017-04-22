using System.Collections.Generic;

namespace SyncTool.Git.Common
{

    public interface IRepositoryPathProvider 
    {              
        string GetRepositoryPath(string repositoryName);

    }
}