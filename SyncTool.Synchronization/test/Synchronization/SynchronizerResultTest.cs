// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization
{
    /// <summary>
    /// Tests for <see cref="SynchronizerResult"/>
    /// </summary>
    public class SynchronizerResultTest
    {
        readonly SynchronizerResult m_Instance;

        public SynchronizerResultTest()
        {
            m_Instance = new SynchronizerResult(new FilePropertiesComparer());
        }


        [Fact]
        public void ApplyTo_throws_InvalidOperationException_if_set_contains_conflicts()
        {            
            m_Instance.Add(new MultipleVersionSyncConflict(FileMockingHelper.GetMockedFile("file1"), FileMockingHelper.GetMockedFile("file1")));

            Assert.Throws<InvalidOperationException>(() => m_Instance.ApplyTo(new Directory("root"), SyncParticipant.Left));

        }

        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_AddFileSyncAction_if_file_already_exists()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1")
            };

            m_Instance.Add(new AddFileSyncAction(SyncParticipant.Left, FileMockingHelper.GetMockedFile("file1")));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory, SyncParticipant.Left));
        }

        [Fact]
        public void ApplyTo_can_successfully_apply_AddFileSyncAction()
        {
            var directory = new Directory("root");
            
            var newFile = new EmptyFile("file1").WithParent(new NullDirectory("dir1", "dir1"));

            m_Instance.Add(new AddFileSyncAction(SyncParticipant.Left, newFile));

            var newDirectory = m_Instance.ApplyTo(directory, SyncParticipant.Left);

            Assert.True(newDirectory.FileExists("dir1/file1"));
        }


        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_RemoveFileSyncAction_if_file_does_not_exist()
        {
            var directory = new Directory("root");

            var fileToRemove = new EmptyFile(new NullDirectory("", "root"), "file1");
            m_Instance.Add(new RemoveFileSyncAction(SyncParticipant.Left, fileToRemove));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory, SyncParticipant.Left));            
        }

        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_RemoveFileSyncAction_if_file_does_not_match_properties()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1980")}
            };

            var fileToRemove = new EmptyFile(new NullDirectory("", "root"), "file1");
            m_Instance.Add(new RemoveFileSyncAction(SyncParticipant.Left, fileToRemove));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory, SyncParticipant.Left));
        }

        [Fact]
        public void ApplyTo_can_successfully_apply_RemoveFileSyncAction()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1980")}
            };

            var fileToRemove = new EmptyFile(new NullDirectory("", "root"), "file1") { LastWriteTime = DateTime.Parse("01.01.1980") };
            m_Instance.Add(new RemoveFileSyncAction(SyncParticipant.Left, fileToRemove));

            var newDirectory = m_Instance.ApplyTo(directory, SyncParticipant.Left);
            Assert.False(newDirectory.FileExists("file1"));
            Assert.Empty(newDirectory.Files);
        }


        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_ReplaceFileSyncAction_if_file_does_not_exist()
        {
            var directory = new Directory("root");

            var oldVersion = new EmptyFile(new NullDirectory("", "root"), "file1");
            var newVersion = new EmptyFile(new NullDirectory("", "root"), "file1");

            m_Instance.Add(new ReplaceFileSyncAction(SyncParticipant.Left, oldVersion, newVersion));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory, SyncParticipant.Left));
        }

        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_ReplaceFileSyncAction_if_file_does_not_match_properties()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1980")}
            };

            var oldVersion = new EmptyFile(new NullDirectory("", "root"), "file1");
            var newVersion = new EmptyFile(new NullDirectory("", "root"), "file1");

            m_Instance.Add(new ReplaceFileSyncAction(SyncParticipant.Left, oldVersion, newVersion));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory, SyncParticipant.Left));
        }



        [Fact]
        public void ApplyTo_can_successfully_apply_ReplaceFileSyncAction()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1980")} 
            };

            var oldVersion = new EmptyFile(new NullDirectory("", "root"), "file1") { LastWriteTime = DateTime.Parse("01.01.1980") };
            var newVersion = new EmptyFile(new NullDirectory("", "root"), "file1") { LastWriteTime = DateTime.Parse("01.01.1981") };
            m_Instance.Add(new ReplaceFileSyncAction(SyncParticipant.Left, oldVersion, newVersion));

            var newDirectory = m_Instance.ApplyTo(directory, SyncParticipant.Left);
            Assert.True(newDirectory.FileExists("file1"));
            Assert.Equal(newVersion.LastWriteTime, newDirectory.GetFile("file1").LastWriteTime);
        }


        [Fact]
        public void ApplyTo_only_applies_changes_with_the_specified_target()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1") {LastWriteTime = DateTime.Parse("01.01.1980")}
            };

            var fileToRemove = new EmptyFile(new NullDirectory("", "root"), "file1") { LastWriteTime = DateTime.Parse("01.01.1980") };
            m_Instance.Add(new RemoveFileSyncAction(SyncParticipant.Left, fileToRemove));

            var newDirectory = m_Instance.ApplyTo(directory, SyncParticipant.Right);
            
            // as we choose target "Right", the directory has to be unchanged
            FileSystemAssert.DirectoryEqual(directory, newDirectory);
        }
    }
}