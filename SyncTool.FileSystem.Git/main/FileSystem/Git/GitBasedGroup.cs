// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using LibGit2Sharp;
using SyncTool.Common;
using SyncTool.FileSystem.Git.Utilities;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedGroup : IGroup, IDisposable
    {
        protected readonly Repository m_Repository;


        public string Name
        {
            get
            {
                var infoFile = (IReadableFile)GetConfigurationRootDirectory().GetFile(RepositoryInfoFile.RepositoryInfoFileName);
                using (var stream = infoFile.OpenRead())
                {
                    var repositoryInfo = stream.Deserialize<RepositoryInfo>();
                    return repositoryInfo.RepositoryName;
                }
            }
        }



        public GitBasedGroup(string repositoryPath)
        {
            if (repositoryPath == null)
            {
                throw new ArgumentNullException(nameof(repositoryPath));
            }

            m_Repository = new Repository(repositoryPath);
        }


        public void Dispose()
        {
            m_Repository.Dispose();
        }



        Commit GetConfigurationCommit() => m_Repository.Branches[RepositoryInitHelper.ConfigurationBranchName].Tip;

        protected GitDirectory GetConfigurationRootDirectory() => new GitDirectory(null, "root", GetConfigurationCommit());
    }
}