﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.FileSystem.Versioning
{

    /// <summary>
    /// Tests for <see cref="ChangeList"/>
    /// </summary>
    public class ChangeListTest
    {
        [Fact(DisplayName = nameof(ChangeList) + " Constructor throws " + nameof(ArgumentNullException)  + " if changes is null")]
        public void Construtor_throws_ArgumentNullException_if_changes_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ChangeList(null));
        }

        [Fact(DisplayName = nameof(ChangeList) + " Constructor throws " + nameof(ArgumentException) + " if changes is empty")]
        public void Construtor_throws_ArgumentNullException_if_changes_is_empty()
        {
            Assert.Throws<ArgumentException>(() => new ChangeList(Enumerable.Empty<IChange>()));
        }

        [Fact(DisplayName = nameof(ChangeList) + " Constructor throws " + nameof(ArgumentException) + " if changes have different paths")]
        public void Construtor_throws_ArgumentNullException_if_changes_have_different_paths()
        {
            var file1 = FileMockingHelper.GetMockedFile("path1", DateTime.MinValue, 23);
            var file2 = FileMockingHelper.GetMockedFile("path2", DateTime.MinValue, 23);

            var change1 = new Change(ChangeType.Added, null, file1);
            var change2 = new Change(ChangeType.Deleted, file2,  null);

            Assert.Throws<ArgumentException>(() => new ChangeList(new [] { change1, change2 }));
        }
    }
}