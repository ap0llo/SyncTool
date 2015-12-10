// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class SingleDirectoryRepositoryPathProvider : IRepositoryPathProvider
    {
        readonly string m_Directory;
       

        public IEnumerable<string> RepositoryPaths => System.IO.Directory.GetDirectories(m_Directory).Where(IsGitRepository).ToArray();


        public SingleDirectoryRepositoryPathProvider(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!System.IO.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"The directory '{directory}' does not exist");
            }
            
            m_Directory = directory;
        }



        public string GetRepositoryPath(string repositoryName) => Path.Combine(m_Directory, repositoryName);
       


        

        bool IsGitRepository(string path)
        {
            try
            {
                using (var repository = new Repository(path))
                {
                    return repository.Info.IsBare;
                }
            }
            catch (RepositoryNotFoundException)
            {
                return false;
            }
        }

    }
}