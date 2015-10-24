//using System;
//using System.Diagnostics;
//using System.Linq;
//using Fluent.IO;
//using LibGit2Sharp;
//using SyncTool.Utilities;
//using Xunit;
//
//namespace SyncTool.FileSystem.Git.Test
//{
//    public class GitBasedFileSystemSnapshotTest : TemporaryDirectory
//    {
//        private const string s_DummyFileName = "dummy.txt";
//        
//
//        public GitBasedFileSystemSnapshotTest()
//        {
//            // create empty bare repository 
//            Repository.Init(DirectoryInfo.FullName, true);
//
//            // clone repo so we can make edits
//            using (var tempDirectory = new TemporaryDirectory())
//            {
//                var clonedRepoPath = Repository.Clone(DirectoryInfo.FullName, tempDirectory.DirectoryInfo.FullName);
//
//                // add a empty file to the repository
//                tempDirectory.CreateFiles(s_DummyFileName);
//
//                // commit and push the file to the bare repository we created
//                using (var clonedRepo = new Repository(clonedRepoPath))
//                {
//                    clonedRepo.Stage(s_DummyFileName);    
//                    clonedRepo.Commit("Initial Commit", NewSignature(), NewSignature(), new CommitOptions());
//                    
//                    clonedRepo.Network.Push(clonedRepo.Network.Remotes["origin"], @"refs/heads/master");
//                }
//            }
//        }
//
//
//
//        [Fact]
//        public void Reading_from_empty_commit_returns_empty_state()
//        {     
//            // the repo contains only a single commit
//            // read this commit as snapshot   
//            using (var repo = new Repository(DirectoryInfo.FullName))
//            {
//                IFileSystemSnapshot instance = new GitBasedFileSystemSnapshot(repo.Head.Tip);
//
//                Assert.Empty(instance.RootDirectory.Directories);
//                Assert.Empty(instance.RootDirectory.Files);
//            }
//        }
//
//
//        [Fact]
//        public void Create_returns_a_new_snapshot()
//        {
//            using (var tempDirectory = new TemporaryDirectory())
//            {
//                tempDirectory
//                    .CreateDirectory("dir1")
//                    .CreateDirectory("dir11")
//                    .CreateFiles("file1", "file2", "file3");
//
//                tempDirectory
//                    .CreateDirectory("dir2")
//                    .CreateFiles("file4");
//
//                var directory = new LocalFileSystemLoader(tempDirectory.DirectoryInfo.FullName).LoadFileSystem();
//
//
//                using (var repo = new Repository(DirectoryInfo.FullName))
//                {
//                    var snapshot = GitBasedFileSystemSnapshot.Create(repo, repo.Branches["master"], directory);
//                    
//                    Assert.NotNull(snapshot);
//                }
//            }
//
//        }
//
//
//        Signature NewSignature() => new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);
//    }
//}