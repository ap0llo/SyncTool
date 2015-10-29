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

namespace SyncTool.FileSystem.Git.Test
{
    /// <summary>
    /// Test for <see cref="TemporaryWorkingDirectory"/>. 
    /// </summary>
    /// <remarks>Assumes git executable is on PATH</remarks>
    public class TemporaryWorkingDirectoryTest : IDisposable
    {
        readonly LocalItemCreator m_LocalItemCreator = new LocalItemCreator();

        readonly TemporaryLocalDirectory m_MasterRepository;
        readonly TemporaryLocalDirectory m_BareMasterRepository;

        public TemporaryWorkingDirectoryTest()
        {
            m_MasterRepository = m_LocalItemCreator.CreateTemporaryDirectory();
            m_BareMasterRepository = m_LocalItemCreator.CreateTemporaryDirectory();

            Process.Start(new ProcessStartInfo("git", "init") {WorkingDirectory = m_MasterRepository.Location, WindowStyle = ProcessWindowStyle.Hidden}).WaitForExit();
            IOFile.WriteAllText(Path.Combine(m_MasterRepository.Location, "dummy.txt"), "hello World!");

            Process.Start(new ProcessStartInfo("git", "add dummy.txt") {WorkingDirectory = m_MasterRepository.Location, WindowStyle = ProcessWindowStyle.Hidden}).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "commit -m Commit") {WorkingDirectory = m_MasterRepository.Location, WindowStyle = ProcessWindowStyle.Hidden}).WaitForExit();

            Process.Start(new ProcessStartInfo("git", $"clone \"{m_MasterRepository.Location}\" \"{m_BareMasterRepository.Location}\" --bare") {WorkingDirectory = m_MasterRepository.Location, WindowStyle = ProcessWindowStyle.Hidden}).WaitForExit();
        }


        [Fact]
        public void Constructor_clones_the_repository_and_checks_out_a_working_copy()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                Assert.True(IODirectory.Exists(instance.Location));
                Assert.True(IODirectory.Exists(Path.Combine(instance.Location, ".git")));
                Assert.True(IOFile.Exists(Path.Combine(instance.Location, "dummy.txt")));                
            }

        }

        [Fact]
        public void Dispose_deletes_the_working_directory()
        {
            var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master");
            Assert.True(IODirectory.Exists(instance.Location));
            instance.Dispose();
            Assert.False(IODirectory.Exists(instance.Location));
        }

        [Fact]
        public void HasChanges_Returns_False_if_no_changes_were_made()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                Assert.False(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_changes_were_made()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                var filePath = Path.Combine(instance.Location, "file1");
                IOFile.WriteAllText(filePath, "Hello World");

                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_deleted()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                IOFile.Delete(Path.Combine(instance.Location, "dummy.txt"));

                Assert.False(IOFile.Exists(Path.Combine(instance.Location, "dummy.txt")));
                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_added()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                CreateFile(instance, "file2.txt");

                Assert.True(IOFile.Exists(Path.Combine(instance.Location, "file2.txt")));
                Assert.True(instance.HasChanges);
            }
        }


        [Fact]
        public void HasChanges_Returns_False_after_Commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
            {
                CreateFile(instance, "file2.txt");

                instance.Commit();

                Assert.False(instance.HasChanges);
            }
        }


        [Fact]
        public void Commit_creates_a_new_commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Location, "master"))
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
            using (var instance = new TemporaryWorkingDirectory(m_BareMasterRepository.Location, "master"))
            {
                CreateFile(instance, fileName);

                instance.Commit();
                instance.Push();
            }
            
            // clone repo again to check if the file we created shows up in new clone
            using (var workingDirectory = new TemporaryWorkingDirectory(m_BareMasterRepository.Location, "master"))
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