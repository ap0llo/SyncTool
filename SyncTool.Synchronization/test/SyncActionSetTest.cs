﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using SyncTool.FileSystem;
using SyncTool.FileSystem.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization
{
    public class SyncActionSetTest
    {
        readonly SyncActionSet m_Instance;

        public SyncActionSetTest()
        {
            m_Instance = new SyncActionSet(new FilePropertiesComparer());
        }


        [Fact]
        public void ApplyTo_throws_InvalidOperationException_if_set_contains_conflicts()
        {            
            m_Instance.Add(new MultipleVersionConflictSyncAction(MockingHelper.GetMockedFile("file1"), MockingHelper.GetMockedFile("file1")));

            Assert.Throws<InvalidOperationException>(() => m_Instance.ApplyTo(new Directory("root")));

        }

        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_AddFileSyncAction_if_file_already_exists()
        {
            var directory = new Directory("root")
            {
                root => new EmptyFile(root, "file1")
            };

            m_Instance.Add(new AddFileSyncAction(SyncParticipant.Left, MockingHelper.GetMockedFile("file1")));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory));
        }

        [Fact]
        public void ApplyTo_can_successfully_apply_AddFileSyncAction()
        {
            var directory = new Directory("root");
            
            var newFile = new EmptyFile("file1").WithParent(new NullDirectory("dir1", "dir1"));

            m_Instance.Add(new AddFileSyncAction(SyncParticipant.Left, newFile));

            var newDirectory = m_Instance.ApplyTo(directory);

            Assert.True(newDirectory.FileExists("dir1/file1"));
        }


        [Fact]
        public void ApplyTo_throws_NotApplicableException_for_RemoveFileSyncAction_if_file_does_not_exist()
        {
            var directory = new Directory("root");

            var fileToRemove = new EmptyFile(new NullDirectory("", "root"), "file1");
            m_Instance.Add(new RemoveFileSyncAction(SyncParticipant.Left, fileToRemove));

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory));            
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

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory));
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

            var newDirectory = m_Instance.ApplyTo(directory);
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

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory));
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

            Assert.Throws<NotApplicableException>(() => m_Instance.ApplyTo(directory));
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

            var newDirectory = m_Instance.ApplyTo(directory);
            Assert.True(newDirectory.FileExists("file1"));
            Assert.Equal(newVersion.LastWriteTime, newDirectory.GetFile("file1").LastWriteTime);
        }
    }
}