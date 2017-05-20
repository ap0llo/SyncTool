using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

using IOFile = System.IO.File;
using IODirectory = System.IO.Directory;

// ReSharper disable PossibleNullReferenceException

namespace SyncTool.Git.Common
{

    /// <summary>
    /// Test for <see cref="TemporaryWorkingDirectory"/>. 
    /// </summary>
    /// <remarks>Assumes git executable is on PATH</remarks>
    public sealed class TemporaryWorkingDirectoryTest : IDisposable
    {
        const string s_DummyFileName = "dummy.txt";
        const string s_File1 = "file1";

        readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();

        readonly DisposableLocalDirectoryWrapper m_MasterRepository;
        readonly DisposableLocalDirectoryWrapper m_BareMasterRepository;
        

        public TemporaryWorkingDirectoryTest()
        {
            m_MasterRepository = m_LocalItemCreator.CreateTemporaryDirectory();
            m_BareMasterRepository = m_LocalItemCreator.CreateTemporaryDirectory();

            
            void RunGit(string command)
            {
                var startInfo = new ProcessStartInfo("git")
                {
                    Arguments = command,
                    WorkingDirectory = m_MasterRepository.Directory.Location,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new ProcessExecutionException($"Failed to start 'git {command}'");
                }

                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new ProcessExecutionException($"'git {command}' exited with exit code {process.ExitCode}");
                }
            }


            RunGit("init");            
            IOFile.WriteAllText(Path.Combine(m_MasterRepository.Directory.Location, "dummy.txt"), "hello World!");

            RunGit($"add { s_DummyFileName}");            
            RunGit("commit -m Commit");

            RunGit($"clone \"{m_MasterRepository.Directory.Location}\" \"{m_BareMasterRepository.Directory.Location}\" --bare");
        }


        [Fact]
        public void Constructor_clones_the_repository_and_checks_out_a_working_copy()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                Assert.True(IODirectory.Exists(instance.Location));
                Assert.True(IODirectory.Exists(Path.Combine(instance.Location, ".git")));
                Assert.True(IOFile.Exists(Path.Combine(instance.Location, "dummy.txt")));                
            }

        }

        [Fact]
        public void Dispose_deletes_the_working_directory()
        {
            var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master");
            Assert.True(IODirectory.Exists(instance.Location));
            instance.Dispose();
            Assert.False(IODirectory.Exists(instance.Location));
        }

        [Fact]
        public void HasChanges_Returns_False_if_no_changes_were_made()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                Assert.False(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_modified()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                var filePath = Path.Combine(instance.Location, s_File1);
                IOFile.WriteAllText(filePath, "Hello_World");

                Assert.True(instance.HasChanges);

                instance.Commit();

                Assert.False(instance.HasChanges);
                IOFile.WriteAllText(filePath, "Hello World");

                Assert.True(instance.HasChanges);                
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_deleted()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                IOFile.Delete(Path.Combine(instance.Location, "dummy.txt"));

                Assert.False(IOFile.Exists(Path.Combine(instance.Location, "dummy.txt")));
                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_added()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                CreateFile(instance, "file2.txt");

                Assert.True(IOFile.Exists(Path.Combine(instance.Location, "file2.txt")));
                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_False_after_Commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                CreateFile(instance, "file2.txt");

                instance.Commit();

                Assert.False(instance.HasChanges);
            }
        }

        [Fact]
        public void Commit_creates_a_new_commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.Location, "master"))
            {
                CreateFile(instance, "file2.txt");

                var originalCommitCount = GetCommitCount(instance.Location, "master");
           
                instance.Commit();

                var newCommitCount = GetCommitCount(instance.Location, "master");

                Assert.Equal(originalCommitCount +1, newCommitCount);
            }

        }

        [Fact]
        public void Push_transfers_changes_to_master_directory()
        {
            var fileName = Path.GetRandomFileName();
            using (var instance = new TemporaryWorkingDirectory(m_BareMasterRepository.Directory.Location, "master"))
            {
                CreateFile(instance, fileName);

                instance.Commit();
                instance.Push();
            }
            
            // clone repo again to check if the file we created shows up in new clone
            using (var workingDirectory = new TemporaryWorkingDirectory(m_BareMasterRepository.Directory.Location, "master"))
            {
                Assert.True(IOFile.Exists(Path.Combine(workingDirectory.Location, fileName)));
            }
        }


       
        public void Dispose()
        {
            m_MasterRepository.Dispose();
            m_BareMasterRepository.Dispose();
        }


        int GetCommitCount(string repositoryPath, string branchName)
        {
            using (var repo = new Repository(repositoryPath))
            {
                return repo.Branches[branchName].Commits.Count();
            }

        }

        void CreateFile(TemporaryWorkingDirectory workingDirectory, string name)
        {
            using (IOFile.Create(Path.Combine(workingDirectory.Location, name)))
            {

            }
        }

       
    }
}