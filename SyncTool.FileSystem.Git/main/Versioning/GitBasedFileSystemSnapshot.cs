using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using LibGit2Sharp;
using Newtonsoft.Json;
using SyncTool.FileSystem.Local;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        const string s_SnapshotDirectoryName = "Snapshot";


        readonly JsonSerializer m_Serializer = new JsonSerializer();
        readonly MetaFileSystemLoader m_MetaFileSystemLoader = new MetaFileSystemLoader();
        readonly MetaFileSystemToFileSystemConverter m_MetaFileSystemConverter = new MetaFileSystemToFileSystemConverter();
        readonly LocalItemCreator m_DirectoryCreator = new LocalItemCreator();

        readonly Commit m_Commit;
        readonly Lazy<IDirectory> m_Directory;

        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public IDirectory RootDirectory => m_Directory.Value;


        internal Commit Commit => m_Commit;


        public GitBasedFileSystemSnapshot(Commit commit)
        {
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }
            m_Commit = commit;

            m_Directory = new Lazy<IDirectory>(LoadSnapshot);

        }


        
        public static GitBasedFileSystemSnapshot Create(Repository repository, string branchName, Directory rootDirectory)
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
                    commitId = workingRepository.Commit();
                    workingRepository.Push();                    
                }
                else
                {
                    Console.WriteLine("No changes");
                    commitId = repository.Branches[branchName].Tip.Sha;
                }
            }

            

            var commit = repository.Lookup<Commit>(commitId);
            return new GitBasedFileSystemSnapshot(commit);
            
       }

        public static bool IsSnapshot(Commit commit) => commit.Tree[s_SnapshotDirectoryName] != null;

        IDirectory LoadSnapshot()
        {
            // load "raw" directory
            // name of root directory is irrelevant, will be overridden by directory properties file
            var rawDirectory = new GitDirectory("root", m_Commit).GetDirectory(s_SnapshotDirectoryName);
            
            // convert to "meta" file system (replaces IFile instances in the tree with more specific implementation)
            var metaFileSystem = m_MetaFileSystemLoader.Convert(rawDirectory);

            // convert to the originally stored file system (load file and directory properties files in the meta file system)
            return m_MetaFileSystemConverter.Convert(metaFileSystem);
        }

    }
}