//using System;
//using System.IO;
//using System.Linq;
//using LibGit2Sharp;
//using Newtonsoft.Json;
//
//namespace SyncTool.FileSystem.Git
//{
//    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
//    {
//        const string s_FileStateSuffix = ".SyncTool.json";
//
//        static readonly JsonSerializer s_Serializer = new JsonSerializer();
//
//        readonly Commit m_Commit;
//        readonly Lazy<Directory> m_Directory;        
//
//        public string Id => m_Commit.Sha;
//
//        public DateTime CreationTime => m_Commit.Author.When.DateTime;
//
//        public Directory RootDirectory => m_Directory.Value;
//
//
//        public GitBasedFileSystemSnapshot(Commit commit)
//        {
//            if (commit == null)
//            {
//                throw new ArgumentNullException(nameof(commit));
//            }
//            m_Commit = commit;
//            
//            m_Directory = new Lazy<Directory>(() => LoadTree(m_Commit));
//        }
//
//
//
//        Directory LoadTree(Commit commit)
//        {
//            var rootDirectory = new Directory();
//
//            foreach (var treeEntry in commit.Tree)
//            {
//                LoadTreeEntry(treeEntry, rootDirectory);
//            }
//
//            return rootDirectory;
//        }
//
//        void LoadTreeEntry(TreeEntry treeEntry, Directory parent)
//        {
//            switch (treeEntry.TargetType)
//            {
//                case TreeEntryTargetType.Blob:                    
//                    if (treeEntry.Name.EndsWith(s_FileStateSuffix))
//                    {
//                        FileProperties fileProperties;
//                        using (var stream = ((Blob)treeEntry.Target).GetContentStream())
//                        using(var jsonReader = new JsonTextReader(new StreamReader(stream)))
//                        {
//                            fileProperties = s_Serializer.Deserialize<FileProperties>(jsonReader);
//
//                        }   
//                        //TODO                     
////                        var file = new File()
////                        {
////                            Name = treeEntry.Name.Substring(0, treeEntry.Name.Length - s_FileStateSuffix.Length),
////                            LastWriteTime                           = fileProperties.LastWriteTime,
////                            Length = fileProperties.Length
////                        };
////                        parent.Add(file);                        
//                    }
//
//                    break;
//
//                case TreeEntryTargetType.Tree:
//                    var directory = parent.Add(new Directory() { Name = treeEntry.Name });
//                    var subTree = (Tree) treeEntry.Target;
//                    foreach (var subTreeEntry in subTree)
//                    {
//                        LoadTreeEntry(subTreeEntry, directory);
//                    }
//                    break;
//            }
//            
//        }
//
//
//
//        private class FileProperties
//        {
//            public DateTime LastWriteTime { get; }
//
//            public long Length { get; }
//
//            public FileProperties(File file)
//            {
//
//                LastWriteTime = file.LastWriteTime;
//                Length = file.Length;
//            }
//
//            public FileProperties()
//            {                
//            }
//        }
//
//
//
//
//        public static GitBasedFileSystemSnapshot Create(Repository repository, Branch branch, Directory rootDirectory)
//        {
//            string commitId;
//            using (var workingRepository = new TemporaryWorkingDirectory(repository.Info.Location, branch.Name))
//            {
//                
//                SerializeDirecotry(workingRepository.DirectoryInfo, rootDirectory);
//
//
//                if (workingRepository.HasChanges)
//                {
//                    commitId = workingRepository.Commit();
//                    workingRepository.Push();                    
//                }
//                else
//                {
//                    commitId = branch.Tip.Sha;
//                }
//            }
//
//            var commit = repository.Lookup<Commit>(commitId);
//            return new GitBasedFileSystemSnapshot(commit);
//            
//        }
//
//
//        static void SerializeDirecotry(DirectoryInfo outputDirectory, Directory directory)
//        {
//            foreach (var fileOnDisk in outputDirectory.EnumerateFiles().Where(f => f.Name.EndsWith(s_FileStateSuffix)))
//            {
//                fileOnDisk.Delete();
//            }
//
//
//            foreach (var file in directory.Files)
//            {
//                using (var streamWriter = new StreamWriter(System.IO.File.Open(Location.Combine(outputDirectory.FullName, file.Name + s_FileStateSuffix), FileMode.Create)))
//                using (var writer = new JsonTextWriter(streamWriter))
//                {
//                    s_Serializer.Serialize(writer, new FileProperties(file));
//                }
//            }
//
//            foreach (var subDirectory in directory.Directories)
//            {
//                var subDirectoryInfo = outputDirectory.CreateSubdirectory(subDirectory.Name);
//                SerializeDirecotry(subDirectoryInfo, subDirectory);
//            }
//
//           
//        }
//    }
//}