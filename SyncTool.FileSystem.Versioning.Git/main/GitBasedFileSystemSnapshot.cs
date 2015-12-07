// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Runtime.Remoting.Proxies;
using LibGit2Sharp;
using SyncTool.FileSystem.Git;
using SyncTool.FileSystem.Local;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.FileSystem.Versioning.Git
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        const string s_SnapshotDirectoryName = "Snapshot";
        
        readonly MetaFileSystemLoader m_MetaFileSystemLoader = new MetaFileSystemLoader();
        readonly MetaFileSystemToFileSystemConverter m_MetaFileSystemConverter = new MetaFileSystemToFileSystemConverter();        
        readonly Commit m_Commit;
        
        IFileSystemMapping m_MetaFileSystemMapping;
        IDirectory m_GitDirectory;
        IDirectory m_MetaFileSystem;


        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public IDirectory RootDirectory { get; private set; }

        internal Commit Commit => m_Commit;



        public GitBasedFileSystemSnapshot(Commit commit)
        {
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }
            m_Commit = commit;
            LoadSnapshot();
           

        }


        
        public static GitBasedFileSystemSnapshot Create(Repository repository, string branchName, IDirectory rootDirectory)
        {
            var directoryCreator = new LocalItemCreator();
            var metaFileSystemCreator = new FileSystemToMetaFileSystemConverter();

            string commitId;
            using (var workingRepository = new TemporaryWorkingDirectory(repository.Info.Path, branchName))
            {                 
                var snapshotDirectoryPath = Path.Combine(workingRepository.Location, s_SnapshotDirectoryName);
                if (NativeDirectory.Exists(snapshotDirectoryPath))
                {
                    NativeDirectory.Delete(path: snapshotDirectoryPath, recursive:true);
                }

                var metaDirectory = metaFileSystemCreator.CreateMetaDirectory(rootDirectory);
                directoryCreator.CreateDirectoryInPlace(metaDirectory, snapshotDirectoryPath);                

                if (workingRepository.HasChanges)
                {
                    try
                    {
                        commitId = workingRepository.Commit();
                        workingRepository.Push();
                    }
                    catch (EmptyCommitException)
                    {
                        // no changes after all (HasChanges does not seem to be a 100% accurate)
                        commitId = repository.Branches[branchName].Tip.Sha;
                    }
                    
                }
                else
                {                    
                    commitId = repository.Branches[branchName].Tip.Sha;
                }
            }

            

            var commit = repository.Lookup<Commit>(commitId);
            return new GitBasedFileSystemSnapshot(commit);
            
       }

        public static bool IsSnapshot(Commit commit) => commit.Tree[s_SnapshotDirectoryName] != null;


        internal IFile GetFileForGitRelativePath(string relativePath)
        {
            relativePath = relativePath.Replace("\\", "/");

            var file = m_MetaFileSystem.GetFile(relativePath);
            var mappedFile = m_MetaFileSystemMapping.GetMappedFile(file);

            return mappedFile;
        }

        void LoadSnapshot()
        {
            // load "raw" directory
            // name of root directory is irrelevant, will be overridden by directory properties file
            m_GitDirectory = new GitDirectory(null, "root", m_Commit);            
                        
            // convert to "meta" file system (replaces IFile instances in the tree with more specific implementation)
            m_MetaFileSystem = m_MetaFileSystemLoader.Convert(m_GitDirectory);

            // convert to the originally stored file system (load file and directory properties files in the meta file system)
            m_MetaFileSystemMapping = m_MetaFileSystemConverter.Convert(m_MetaFileSystem.GetDirectory(s_SnapshotDirectoryName));

            RootDirectory = m_MetaFileSystemMapping.GetMappedDirectory(m_MetaFileSystem.GetDirectory(s_SnapshotDirectoryName));
        }

    }
}