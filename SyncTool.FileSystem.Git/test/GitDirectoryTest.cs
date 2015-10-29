using System;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using LibGit2Sharp;
using Xunit;

namespace SyncTool.FileSystem.Git.Test
{
    public class GitDirectoryTest : IDisposable
    {
        const string s_DummyFileName = "dummy.txt";
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";
        const string s_File1 = "file1";
        const string s_File2 = "file2";
        readonly TemporaryLocalDirectory m_Repository;
        readonly CreateLocalDirectoryVisitor m_DirectoryCreator;



        public GitDirectoryTest()
        {
            m_DirectoryCreator = new CreateLocalDirectoryVisitor();
            m_Repository = m_DirectoryCreator.CreateTemporaryDirectory();

            // create empty bare repository 
            Repository.Init(m_Repository.Location, true);

            using (var tempDirectory = m_DirectoryCreator.CreateTemporaryDirectory())
            {
                var clonedRepoPath = Repository.Clone(m_Repository.Location, tempDirectory.Location);

                // add a empty file to the repository
                m_DirectoryCreator.CreateFile(new EmptyFile(s_DummyFileName), tempDirectory.Location);

                // commit and push the file to the bare repository we created
                using (var clonedRepo = new Repository(clonedRepoPath))
                {
                    clonedRepo.Stage(s_DummyFileName);                    
                    clonedRepo.Commit("Initial Commit", SignatureHelper.NewSignature(), SignatureHelper.NewSignature(), new CommitOptions());

                    clonedRepo.Network.Push(clonedRepo.Network.Remotes["origin"], @"refs/heads/master");
                }
            }
        }

        
        [Fact]
        public void Repository_with_single_commit_and_single_file()
        {
            using (var repo = new Repository(m_Repository.Location))
            {
                var commit = repo.Commits.Single();

                var gitDirectory = new GitDirectory(m_Repository.Name, commit);

                Assert.Equal(m_Repository.Name, gitDirectory.Name);
                Assert.Empty(gitDirectory.Directories);
                Assert.Single(gitDirectory.Files);
                Assert.True(gitDirectory.FileExists(s_DummyFileName));
            }
        }

        [Fact]
        public void Repository_with_subdirectories()
        {
            // arrange
            string commitId;
            using (var workingDirectory = new TemporaryWorkingDirectory(m_Repository.Location, "master"))
            {
                
                var directory = new Directory(Path.GetFileName(workingDirectory.Location))
                {
                    new Directory(s_Dir1)
                    {
                        new EmptyFile(s_File1),
                        new EmptyFile(s_File2)
                    },
                    new Directory(s_Dir2)
                    {
                        new EmptyFile(s_File1)
                    }
                };

                System.IO.File.Delete(Path.Combine(workingDirectory.Location, s_DummyFileName));

                m_DirectoryCreator.CreateDirectory(directory, Path.GetDirectoryName(workingDirectory.Location));
                commitId = workingDirectory.Commit();
                workingDirectory.Push();
            }

            using (var repo = new Repository(m_Repository.Location))
            {
                // act
                var commit = repo.Lookup<Commit>(commitId);
                var gitDirectory = new GitDirectory(m_Repository.Name, commit);


                // assert
                Assert.Equal(m_Repository.Name, gitDirectory.Name);

                Assert.Equal(2, gitDirectory.Directories.Count());
                Assert.Empty(gitDirectory.Files);

                Assert.True(gitDirectory.DirectoryExists(s_Dir1));
                Assert.True(gitDirectory.GetDirectory(s_Dir1).FileExists(s_File1));
                Assert.True(gitDirectory.GetDirectory(s_Dir1).FileExists(s_File2));
                Assert.True(gitDirectory.DirectoryExists(s_Dir2));
                Assert.True(gitDirectory.GetDirectory(s_Dir2).FileExists(s_File1));
                
            }

        }

        public void Dispose()
        {
            m_Repository.Dispose();
        }
    }
}