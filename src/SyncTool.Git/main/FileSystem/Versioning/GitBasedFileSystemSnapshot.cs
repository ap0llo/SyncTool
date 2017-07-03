using System;
using System.IO;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;
using SyncTool.FileSystem.Versioning.MetaFileSystem;
using SyncTool.Git.RepositoryAccess;

namespace SyncTool.Git.FileSystem.Versioning
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        public const string SnapshotDirectoryName = "Snapshot";
        
        readonly MetaFileSystemLoader m_MetaFileSystemLoader = new MetaFileSystemLoader();
        readonly MetaFileSystemToFileSystemConverter m_MetaFileSystemConverter = new MetaFileSystemToFileSystemConverter();        
        readonly Commit m_Commit;
        readonly Lazy<IDirectory> m_RootDirectory;


        public IFileSystemHistory History { get; }

        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public IDirectory RootDirectory => m_RootDirectory.Value;

        internal Commit Commit => m_Commit;

        
        public GitBasedFileSystemSnapshot(IFileSystemHistory history, Commit commit)
        {
            History = history ?? throw new ArgumentNullException(nameof(history));
            m_Commit = commit ?? throw new ArgumentNullException(nameof(commit));
            m_RootDirectory = new Lazy<IDirectory>(LoadRootDirectory);
        }

        
        public static GitBasedFileSystemSnapshot Create(WorkingDirectoryFactory workingDirectoryFactory, Repository repository, BranchName branchName, IFileSystemHistory history, IDirectory rootDirectory)
        {
            var directoryCreator = new LocalItemCreator();
            var metaFileSystemCreator = new FileSystemToMetaFileSystemConverter();

            var branch = repository.GetBranch(branchName);

            string commitId;
            using (var workingRepository = workingDirectoryFactory.CreateTemporaryWorkingDirectory(repository.Info.Path, branch.FriendlyName))
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

        IDirectory LoadRootDirectory()
        {
            // load "raw" directory
            // name of root directory is irrelevant, will be overridden by directory properties file
            var gitDirectory = new GitDirectory(null, "root", m_Commit);            
                        
            // convert to "meta" file system (replaces IFile instances in the tree with more specific implementation)
            var metaFileSystem = m_MetaFileSystemLoader.Convert(gitDirectory);

            // convert to the originally stored file system (load file and directory properties files in the meta file system)
            return m_MetaFileSystemConverter.Convert(metaFileSystem.GetDirectory(SnapshotDirectoryName));            
        }
    }
}