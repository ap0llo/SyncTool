using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Utilities;
using Xunit;

// ReSharper disable PossibleNullReferenceException

namespace SyncTool.FileSystem.Git.Test
{
    /// <summary>
    /// Test for <see cref="TemporaryWorkingDirectory"/>. 
    /// </summary>
    /// <remarks>Assumes git executable is on PATH</remarks>
    public class TemporaryWorkingDirectoryTest : IDisposable
    {
        readonly TemporaryDirectory m_MasterRepository = new TemporaryDirectory();        
        readonly TemporaryDirectory m_BareMasterRepository = new TemporaryDirectory();

        public TemporaryWorkingDirectoryTest()
        {
            Process.Start(new ProcessStartInfo("git", "init") {WorkingDirectory = m_MasterRepository.Directory.FullName, WindowStyle = ProcessWindowStyle.Hidden}).WaitForExit();
            File.WriteAllText(Path.Combine(m_MasterRepository.Directory.FullName, "dummy.txt"), "hello World!");

            Process.Start(new ProcessStartInfo("git", "add dummy.txt") {WorkingDirectory = m_MasterRepository.Directory.FullName, WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
            Process.Start(new ProcessStartInfo("git", "commit -m Commit") { WorkingDirectory = m_MasterRepository.Directory.FullName, WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();

            Process.Start(new ProcessStartInfo("git", $"clone \"{m_MasterRepository.Directory.FullName}\" \"{m_BareMasterRepository.Directory.FullName}\" --bare") {WorkingDirectory = m_MasterRepository.Directory.FullName, WindowStyle = ProcessWindowStyle.Hidden }).WaitForExit();
        }


        [Fact]
        public void Constructor_clones_the_repository_and_checks_out_a_working_copy()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                Assert.True(instance.Directory.Exists);
                Assert.True(Directory.Exists(Path.Combine(instance.Directory.FullName, ".git")));
                Assert.True(File.Exists(Path.Combine(instance.Directory.FullName, "dummy.txt")));                
            }

        }

        [Fact]
        public void Dispose_deltes_the_working_directory()
        {
            var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master");
            Assert.True(instance.Directory.Exists);
            instance.Dispose();
            Assert.False(instance.Directory.Exists);
        }

        [Fact]
        public void HasChanges_Returns_False_if_no_changes_were_made()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                Assert.False(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_changes_were_made()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                var filePath = Path.Combine(instance.Directory.FullName, "file1");
                File.WriteAllText(filePath, "Hello World");

                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_deleted()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                File.Delete(Path.Combine(instance.Directory.FullName, "dummy.txt"));

                Assert.False(File.Exists(Path.Combine(instance.Directory.FullName, "dummy.txt")));
                Assert.True(instance.HasChanges);
            }
        }

        [Fact]
        public void HasChanges_Returns_True_if_a_file_was_added()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                CreateFile(instance, "file2.txt");

                Assert.True(File.Exists(Path.Combine(instance.Directory.FullName, "file2.txt")));
                Assert.True(instance.HasChanges);
            }
        }


        [Fact]
        public void HasChanges_Returns_False_after_Commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                CreateFile(instance, "file2.txt");

                instance.Commit();

                Assert.False(instance.HasChanges);
            }
        }


        [Fact]
        public void Commit_creates_a_new_commit()
        {
            using (var instance = new TemporaryWorkingDirectory(m_MasterRepository.Directory.FullName, "master"))
            {
                CreateFile(instance, "file2.txt");

                var originalCommitCount = GetCommitCount(instance.Directory.FullName, "master");
           
                instance.Commit();

                var newCommitCount = GetCommitCount(instance.Directory.FullName, "master");

                Assert.Equal(originalCommitCount +1, newCommitCount);
            }

        }


        [Fact]
        public void Push_transfers_changes_to_master_directory()
        {
            var fileName = Path.GetRandomFileName();
            using (var instance = new TemporaryWorkingDirectory(m_BareMasterRepository.Directory.FullName, "master"))
            {
                CreateFile(instance, fileName);

                instance.Commit();
                instance.Push();
            }
            
            // clone repo again to check if the file we created shows up in new clone
            using (var workingDirectory = new TemporaryWorkingDirectory(m_BareMasterRepository.Directory.FullName, "master"))
            {
                Assert.True(File.Exists(Path.Combine(workingDirectory.Directory.FullName, fileName)));
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
            using (File.Create(Path.Combine(workingDirectory.Directory.FullName, name)))
            {

            }
        }



    }
}