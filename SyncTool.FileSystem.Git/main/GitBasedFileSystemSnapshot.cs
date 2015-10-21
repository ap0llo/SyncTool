using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedFileSystemSnapshot : IFileSystemSnapshot
    {
        const string s_FileStateSuffix = ".SyncTool.json";

        static readonly JsonSerializer s_Serializer = new JsonSerializer();

        readonly Commit m_Commit;
        readonly Lazy<Directory> m_Directory;        

        public string Id => m_Commit.Sha;

        public DateTime CreationTime => m_Commit.Author.When.DateTime;

        public Directory RootDirectory => m_Directory.Value;


        public GitBasedFileSystemSnapshot(Commit commit)
        {
            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }
            m_Commit = commit;
            
            m_Directory = new Lazy<Directory>(() => LoadTree(m_Commit));
        }



        private Directory LoadTree(Commit commit)
        {
            var rootDirectory = new Directory();

            foreach (var treeEntry in commit.Tree)
            {
                LoadTreeEntry(treeEntry, rootDirectory);
            }

            return rootDirectory;
        }

        private void LoadTreeEntry(TreeEntry treeEntry, Directory parent)
        {
            switch (treeEntry.TargetType)
            {
                case TreeEntryTargetType.Blob:                    
                    if (treeEntry.Name.EndsWith(s_FileStateSuffix))
                    {
                        FileProperties fileProperties;
                        using (var stream = ((Blob)treeEntry.Target).GetContentStream())
                        using(var jsonReader = new JsonTextReader(new StreamReader(stream)))
                        {
                            fileProperties = s_Serializer.Deserialize<FileProperties>(jsonReader);

                        }                        
                        var file = new File()
                        {
                            Name = treeEntry.Name.Substring(0, treeEntry.Name.Length - s_FileStateSuffix.Length),
                            LastWriteTime                           = fileProperties.LastWriteTime,
                            Length = fileProperties.Length
                        };
                        parent.Add(file);                        
                    }

                    break;
                case TreeEntryTargetType.Tree:
                    var directory = parent.Add(new Directory() { Name = treeEntry.Name });
                    var subTree = (Tree) treeEntry.Target;
                    foreach (var subTreeEntry in subTree)
                    {
                        LoadTreeEntry(subTreeEntry, directory);
                    }
                    break;
            }
            
        }



        private class FileProperties
        {
            public DateTime LastWriteTime { get; set; }

            public long Length { get; set; }

            public FileProperties(File file)
            {

                this.LastWriteTime = file.LastWriteTime;
                this.Length = file.Length;

            }

            public FileProperties()
            {
                
            }
        }




        public static GitBasedFileSystemSnapshot Create(Repository repository, Branch branch, Directory rootDirectory)
        {
            var treeDefinition = new TreeDefinition();

            var files = rootDirectory.GetFlatFileList().ToList();

            //TODO: handle deleted files
            foreach (var file in files)
            {
                //TODO: serialize file info
                using (var memoryStream = new MemoryStream(new byte[256]))
                {
//                    using(var streamWriter = new StreamWriter(memoryStream))
//                    using (var writer = new JsonTextWriter(streamWriter))
//                    {
//                        s_Serializer.Serialize(writer, new FileProperties(file));
//                    }
//
//
//                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var blob = repository.ObjectDatabase.CreateBlob(memoryStream);
                    treeDefinition.Add(file.Path, blob, Mode.NonExecutableFile);
                }
            }

            var parentCommit = branch.Tip;

            var tree = repository.ObjectDatabase.CreateTree(treeDefinition);

            Signature committer = new Signature("John Doe", "somebody@example.com", DateTime.Now);

            Commit commit = repository.ObjectDatabase.CreateCommit(committer, committer, "", tree, new[] {parentCommit}, false);
            
            return new GitBasedFileSystemSnapshot(commit);
            
        }
    }
}