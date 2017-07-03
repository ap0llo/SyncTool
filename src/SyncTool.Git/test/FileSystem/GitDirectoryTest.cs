using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Git.RepositoryAccess;
using Xunit;
using Directory = SyncTool.FileSystem.Directory;

namespace SyncTool.Git.FileSystem
{
    /// <summary>
    /// Tests for <see cref="GitDirectory"/>
    /// </summary>
    public sealed class GitDirectoryTest : IDisposable
    {
        const string s_DummyFileName = "dummy.txt";
        const string s_Dir1 = "dir1";
        const string s_Dir2 = "dir2";
        const string s_File1 = "file1";
        const string s_File2 = "file2";
        readonly DisposableLocalDirectoryWrapper m_Repository;
        readonly LocalItemCreator m_DirectoryCreator;


        public GitDirectoryTest()
        {
            m_DirectoryCreator = new LocalItemCreator();
            m_Repository = m_DirectoryCreator.CreateTemporaryDirectory();
            
            RepositoryInitHelper.InitializeRepository(m_Repository.Directory.Location);            
        }


        [Fact]
        public void Repository_with_single_commit_and_single_file()
        {
            using (var repo = new Repository(m_Repository.Directory.Location))
            {
                var commit = repo.Commits.Single();

                var gitDirectory = new GitDirectory(null, m_Repository.Directory.Name, commit);

                Assert.Equal(m_Repository.Directory.Name, gitDirectory.Name);
                Assert.Empty(gitDirectory.Directories);
                Assert.Single(gitDirectory.Files);
                Assert.True(gitDirectory.FileExists(RepositoryInfoFile.RepositoryInfoFileName));
            }
        }

        [Fact]
        public void Repository_with_subdirectories()
        {
            // arrange
            string commitId;
            using (var workingDirectory = new TemporaryWorkingDirectory(null, m_Repository.Directory.Location, "master"))
            {                
                var directory = new Directory(Path.GetFileName(workingDirectory.Location))
                {
                    root => new Directory(root, s_Dir1)
                    {
                        dir1 => new EmptyFile(dir1, s_File1),
                        dir1 => new EmptyFile(dir1, s_File2)
                    },
                    root => new Directory(root, s_Dir2)
                    {
                        dir2 => new EmptyFile(dir2, s_File1)
                    }
                };

                System.IO.File.Delete(Path.Combine(workingDirectory.Location, RepositoryInfoFile.RepositoryInfoFileName));

                m_DirectoryCreator.CreateDirectory(directory, Path.GetDirectoryName(workingDirectory.Location));
                commitId = workingDirectory.Commit();
                workingDirectory.Push();
            }

            using (var repo = new Repository(m_Repository.Directory.Location))
            {
                // act
                var commit = repo.Lookup<Commit>(commitId);
                var gitDirectory = new GitDirectory(null, m_Repository.Directory.Name, commit);


                // assert
                Assert.Equal(m_Repository.Directory.Name, gitDirectory.Name);

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