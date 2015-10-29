using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using LibGit2Sharp;
using Newtonsoft.Json;
using NativeDirectory = System.IO.Directory;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        const string s_SnapshotDirectoryName = "Snapshot";


        readonly JsonSerializer m_Serializer = new JsonSerializer();
        readonly MetaFileSystemLoader m_MetaFileSystemLoader = new MetaFileSystemLoader();
        readonly MetaFileSystemToFileSystemConverter m_MetaFileSystemConverter = new MetaFileSystemToFileSystemConverter();
        readonly CreateLocalDirectoryVisitor m_DirectoryCreator = new CreateLocalDirectoryVisitor();

        readonly Commit m_Commit;
        readonly Lazy<IDirectory> m_Directory;

        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public IDirectory RootDirectory => m_Directory.Value;


        public GitBasedFileSystemSnapshot(Commit commit)
        {
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }
            m_Commit = commit;

            m_Directory = new Lazy<IDirectory>(LoadSnapshot);

        }


        
        public static GitBasedFileSystemSnapshot Create(Repository repository, Branch branch, Directory rootDirectory)
        {
            var directoryCreator = new CreateLocalDirectoryVisitor();
            var metaFileSystemCreator = new MetaFileSystemCreator();

            string commitId;
            using (var workingRepository = new TemporaryWorkingDirectory(repository.Info.Path, branch.Name))
            {

                System.Diagnostics.Process.Start(workingRepository.Location);

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
                    commitId = branch.Tip.Sha;
                }
            }

            var commit = repository.Lookup<Commit>(commitId);
            return new GitBasedFileSystemSnapshot(commit);
            
       }


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