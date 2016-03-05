// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using SyncTool.Git.FileSystem;

namespace SyncTool.Git.Common
{
    public static class RepositoryInitHelper
    {
        
        // TODO: Move to RepositoryConstants or something similar
        public const string InitialCommitTagName = "InitialCommit";
        public const string ConfigurationBranchName = "Configuration";



        /// <summary>
        /// Initializes a new bare repository at the specified location, adds a repository info file to the root directory
        /// and tags the initial commit with he value of <see cref="InitialCommitTagName"/>
        /// </summary>
        public static void InitializeRepository(string location, string repositoryName = null)
        {            
            // initialize a bare repository
            Repository.Init(location, true);

            var directoryCreator = new LocalItemCreator();

            // clone the repository, add initial commit and push the changes back to the actual repository
            using (var tempDirectory = directoryCreator.CreateTemporaryDirectory())
            {
                var clonedRepoPath = Repository.Clone(location, tempDirectory.Directory.Location);

                var repositoryInfoFile = new RepositoryInfoFile(tempDirectory.Directory, repositoryName);

                // add a empty file to the repository
                directoryCreator.CreateFile(repositoryInfoFile, tempDirectory.Directory.Location);

                // commit and push the file to the bare repository we created
                using (var clonedRepo = new Repository(clonedRepoPath))
                {
                    var signature = SignatureHelper.NewSignature();

                    clonedRepo.Stage(repositoryInfoFile.Name);
                    clonedRepo.Commit("Initial Commit", signature, signature, new CommitOptions());                    

                    clonedRepo.Network.Push(clonedRepo.Network.Remotes["origin"], @"refs/heads/master");                    
                }
            }

            //create the configuration branch pointing to the initial commit
            using (var repository = new Repository(location))
            {
                repository.CreateBranch(ConfigurationBranchName, repository.GetAllCommits().Single());
                repository.Tags.Add(InitialCommitTagName, repository.GetAllCommits().Single());
            }
        }
        
             
    }
}