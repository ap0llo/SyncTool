// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using LibGit2Sharp;
using SyncTool.FileSystem.Local;

namespace SyncTool.FileSystem.Git
{
    static class RepositoryInitHelper
    {
        

        public const string InitialCommitTagName = "InitialCommit";

        /// <summary>
        /// Initializes a new bare repository at the specified location, adds a repository info file to the root directory
        /// and tags the initial commit with he value of <see cref="InitialCommitTagName"/>
        /// </summary>
        public static void InitializeRepository(string location)
        {            
            // initialize a bare repository
            Repository.Init(location, true);

            var directoryCreator = new LocalItemCreator();

            // clone the repository, add initial commit and push the changes back to the actual repository
            using (var tempDirectory = directoryCreator.CreateTemporaryDirectory())
            {
                var clonedRepoPath = Repository.Clone(location, tempDirectory.Directory.Location);

                var repositoryInfoFile = new RepositoryInfoFile(tempDirectory.Directory);

                // add a empty file to the repository
                directoryCreator.CreateFile(repositoryInfoFile, tempDirectory.Directory.Location);

                // commit and push the file to the bare repository we created
                using (var clonedRepo = new Repository(clonedRepoPath))
                {
                    var signature = SignatureHelper.NewSignature();

                    clonedRepo.Stage(repositoryInfoFile.Name);
                    var commit = clonedRepo.Commit("Initial Commit", signature, signature, new CommitOptions());
                    clonedRepo.ApplyTag(InitialCommitTagName, commit.Sha);

                    clonedRepo.Network.Push(clonedRepo.Network.Remotes["origin"], @"refs/heads/master");
                    clonedRepo.Network.Push(clonedRepo.Network.Remotes["origin"], @"refs/tags/" + InitialCommitTagName);
                }
            }
        }
        
             
    }
}