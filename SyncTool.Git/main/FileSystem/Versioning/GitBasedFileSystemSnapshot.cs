// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem.Versioning.MetaFileSystem;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        public const string SnapshotDirectoryName = "Snapshot";
        
        readonly MetaFileSystemLoader m_MetaFileSystemLoader = new MetaFileSystemLoader();
        readonly MetaFileSystemToFileSystemConverter m_MetaFileSystemConverter = new MetaFileSystemToFileSystemConverter();        
        readonly Commit m_Commit;
                
        IDirectory m_GitDirectory;
        IDirectory m_MetaFileSystem;


        public IFileSystemHistory History { get; }

        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public IDirectory RootDirectory { get; private set; }

        internal Commit Commit => m_Commit;



        public GitBasedFileSystemSnapshot(IFileSystemHistory history, Commit commit)
        {
            if (history == null)
            {
                throw new ArgumentNullException(nameof(history));
            }
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            History = history;
            m_Commit = commit;
            LoadSnapshot();          
        }


        
        public static GitBasedFileSystemSnapshot Create(Repository repository, BranchName branchName, IFileSystemHistory history, IDirectory rootDirectory)
        {
            var directoryCreator = new LocalItemCreator();
            var metaFileSystemCreator = new FileSystemToMetaFileSystemConverter();

            var branch = repository.GetBranch(branchName);

            string commitId;
            using (var workingRepository = new TemporaryWorkingDirectory(repository.Info.Path, branch.FriendlyName))
            {                 
                var metaDirectory = metaFileSystemCreator.CreateMetaDirectory(rootDirectory);
               
                var snapshotDirectoryPath = Path.Combine(workingRepository.Location, SnapshotDirectoryName);
                directoryCreator.CreateDirectoryInPlace(metaDirectory, snapshotDirectoryPath, true);                

                if (workingRepository.HasChanges)
                {
                    try
                    {
                        commitId = workingRepository.Commit($"Created snapshot in '{branchName}'");
                        workingRepository.Push();
                    }
                    catch (EmptyCommitException)
                    {
                        // no changes after all (HasChanges does not seem to be a 100% accurate)
                        commitId = repository.GetBranch(branchName).Tip.Sha;
                    }                    
                }
                else
                {                    
                    commitId = repository.GetBranch(branchName).Tip.Sha;
                }
            }            

            var commit = repository.Lookup<Commit>(commitId);
            return new GitBasedFileSystemSnapshot(history, commit);
            
       }

        public static bool IsSnapshot(Commit commit) => commit.Tree[SnapshotDirectoryName] != null;


        internal IFile GetFileForGitPath(string relativePath)
        {
            var filePath = GetPathForGitPath(relativePath);
            return RootDirectory.GetFile(filePath);            
        }        

        /// <summary>
        /// Translates the path of a git-tracked fileto the path of the corresponding file in the snapshot
        /// </summary>
        internal static string GetPathForGitPath(string path)
        {
            return path
                // normalize slashes
                .Replace("\\", "/")
                // remove suffix added to the file name
                .Substring(0, path.Length - FilePropertiesFile.FileNameSuffix.Length)
                // remove the directory name at the beginning of the file (snapshots are created in a directory within the repository)
                .Remove(0, SnapshotDirectoryName.Length);
        }


        void LoadSnapshot()
        {
            // load "raw" directory
            // name of root directory is irrelevant, will be overridden by directory properties file
            m_GitDirectory = new GitDirectory(null, "root", m_Commit);            
                        
            // convert to "meta" file system (replaces IFile instances in the tree with more specific implementation)
            m_MetaFileSystem = m_MetaFileSystemLoader.Convert(m_GitDirectory);

            // convert to the originally stored file system (load file and directory properties files in the meta file system)
            RootDirectory= m_MetaFileSystemConverter.Convert(m_MetaFileSystem.GetDirectory(SnapshotDirectoryName));

            
        }

    }
}