// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization.Conflicts
{
    public class MultipleVersionSyncConflictTest
    {
        [Fact(DisplayName = nameof(MultipleVersionSyncConflict) + ": Constructor checks parameter for null")]
        public void Constructor_checks_parameter_for_null()
        {
            Assert.Throws<ArgumentNullException>(() => new MultipleVersionSyncConflict(null));
        }

        [Fact(DisplayName = nameof(MultipleVersionSyncConflict) + ": Constructor throws " + nameof(ArgumentException) + " if list of conflicted files contains less than two items")]
        public void Constructor_throws_ArgumentException_if_list_of_conflicted_files_contains_less_than_two_items()
        {
            Assert.Throws<ArgumentException>(() => new MultipleVersionSyncConflict());
            Assert.Throws<ArgumentException>(() => new MultipleVersionSyncConflict(new IFile[0]));
            Assert.Throws<ArgumentException>(() => new MultipleVersionSyncConflict(new IFile[1]));
        }

        [Fact(DisplayName = nameof(MultipleVersionSyncConflict) + ": Constructor throws " + nameof(ArgumentException) + " if the files' paths differ")]
        public void Constructor_throws_ArgumentException_if_the_files_paths_differs()
        {
            var file1 = FileMockingHelper.GetFileMock().Named("file1").WithParentNamed("dir1").Object;
            var file2 = FileMockingHelper.GetFileMock().Named("file1").WithParentNamed("dir2").Object;

            Assert.Throws<ArgumentException>(() => new MultipleVersionSyncConflict(file1, file2));
        }

        [Fact(DisplayName = nameof(MultipleVersionSyncConflict) + ": Constructor: Check of file paths is case-insensitive")]
        public void Constructor_Check_of_file_paths_is_case_insensitive()
        {
            var file1 = FileMockingHelper.GetFileMock().Named("file1").WithParentNamed("dir1").Object;
            var file2 = FileMockingHelper.GetFileMock().Named("fILE1").WithParentNamed("DIR1").Object;

            var conflict = new MultipleVersionSyncConflict(file1, file2);
        }

    }
}