// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;
using SyncTool.Git.Common;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization.Conflicts;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization.Conflicts
{
    /// <summary>
    /// Tests for <see cref="GitConflictService"/>
    /// </summary>
    public class GitConflictServiceTest : GitGroupBasedTest
    {
        readonly GitBasedGroup m_Group;
        readonly GitConflictService m_Service;

        public GitConflictServiceTest()
        {
            m_Group = CreateGroup();
            m_Service = new GitConflictService(m_Group);
        }


        #region Items

        [Fact]
        public void T01_Items_returns_empty_enumerable_if_branch_does_not_exist()
        {            
            Assert.Empty(m_Service.Items);
        }

        [Fact]
        public void T02_Items_returns_empty_enumerable_if_branch_is_empty()
        {
            m_Group.Repository.CreateBranch(GitConflictService.BranchName, m_Group.Repository.GetInitialCommit());
            Assert.Empty(m_Service.Items);
        }

        [Fact]
        public void T03_Items_returns_expected_conflicts()
        {
            var expected = new ConflictInfo("/file1", null);

            m_Service.Add(expected);

            Assert.Single(m_Service.Items);
            var actual = m_Service.Items.Single();
            Assert.Equal(expected.FilePath, actual.FilePath);
            DictionaryAssert.Equal(expected.SnapshotIds, actual.SnapshotIds);
        }


        [Fact]
        public void T04_Items_returns_expected_conflicts()
        {
            var expected = new ConflictInfo("/file1", new Dictionary<string, string>() { {"name", "id"} });

            m_Service.Add(expected);

            Assert.Single(m_Service.Items);
            var actual = m_Service.Items.Single();
            Assert.Equal(expected.FilePath, actual.FilePath);
            DictionaryAssert.Equal(expected.SnapshotIds, actual.SnapshotIds);
        }

        #endregion


        #region Indexer


        [Fact]
        public void T06_Indexer_validates_path()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service[null]);
            Assert.Throws<FormatException>(() => m_Service[""]);
            Assert.Throws<FormatException>(() => m_Service[" "]);
            Assert.Throws<FormatException>(() => m_Service["/"]);
            Assert.Throws<FormatException>(() => m_Service["fileName"]);
            Assert.Throws<FormatException>(() => m_Service["relative/path"]);
        }

        [Fact]
        public void T07_Indexer_throws_ItemNotFoundException_if_branch_does_not_exist()
        {
            Assert.Throws<ItemNotFoundException>(() => m_Service["/my/file"]);
        }

        [Fact]
        public void T08_Indexer_throws_ItemNotFoundException_if_item_does_not_exist()
        {
            var conflict = new ConflictInfo("/file1", null);
            m_Service.Add(conflict);

            Assert.Throws<ItemNotFoundException>(() => m_Service["/some/other/path"]);
        }


        [Fact]
        public void T09_Indexer_returns_expected_result()
        {
            var expected = new ConflictInfo("/file1", null);
            m_Service.Add(expected);

            var actual = m_Service[expected.FilePath];

            Assert.NotNull(actual);
            Assert.Equal(expected.FilePath,actual.FilePath);
            DictionaryAssert.Equal(expected.SnapshotIds, actual.SnapshotIds);
        }

        #endregion


        #region Add

        [Fact]
        public void T08_Add_throws_DuplicateItemException_if_conflict_has_already_been_added()
        {
            var conflict = new ConflictInfo("/file1", null);
            m_Service.Add(conflict);
            Assert.Throws<DuplicateItemException>(() => m_Service.Add(conflict));
        }

        [Fact]
        public void T09_Add_properly_stores_added_conflicts()
        {
            var conflict1 = new ConflictInfo("/file1", null);
            var conflict2 = new ConflictInfo("/file2", null);

            var commitCount = m_Group.Repository.GetAllCommits().Count();

            m_Service.Add(conflict1, conflict2);
            Assert.Equal(commitCount +1 , m_Group.Repository.GetAllCommits().Count());

            var newServiceInstance = new GitConflictService(m_Group);
            Assert.Equal(2, newServiceInstance.Items.Count());            
        }

        #endregion


        #region Remove

        [Fact]
        public void T10_Remove_throws_ItemNotFoundException_if_conflict_does_not_exist()
        {
            var conflict = new ConflictInfo("/file1", null);            
            Assert.Throws<ItemNotFoundException>(() => m_Service.Remove(conflict));
        }

        [Fact]
        public void T11_Remove_removes_the_specified_items()
        {
            var commitCount = m_Group.Repository.GetAllCommits().Count();
            var conflict1 = new ConflictInfo("/file1", null);
            var conflict2 = new ConflictInfo("/file2", null);

            Assert.Empty(m_Service.Items);
            m_Service.Add(conflict1, conflict2);
            Assert.NotEmpty(m_Service.Items);

            m_Service.Remove(conflict1, conflict2);
            Assert.Empty(m_Service.Items);
            Assert.Equal(commitCount + 2, m_Group.Repository.GetAllCommits().Count());
        }

        #endregion

        #region ItemExists

        [Fact]
        public void T12_ItemExists_validates_path()
        {
            Assert.Throws<ArgumentNullException>(() => m_Service.ItemExists(null));
            Assert.Throws<FormatException>(() => m_Service.ItemExists(""));
            Assert.Throws<FormatException>(() => m_Service.ItemExists(" "));
            Assert.Throws<FormatException>(() => m_Service.ItemExists("/"));
            Assert.Throws<FormatException>(() => m_Service.ItemExists("fileName"));
            Assert.Throws<FormatException>(() => m_Service.ItemExists("relative/path"));
        }

        [Fact]
        public void T13_ItemExists_returns_the_expected_result()
        {
            var conflict = new ConflictInfo("/some/file/path", null);
            m_Service.Add(conflict);

            Assert.True(m_Service.ItemExists("/SOME/file/Path"));
            Assert.False(m_Service.ItemExists("/another/path"));
        }


        #endregion


        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }
    }
}