// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    class FilteredFileSystemDiff : IFileSystemDiff
    {
        readonly IFileSystemDiff m_WrappedDiff;
        readonly IEqualityComparer<IFile> m_FileComparer;

        public FilteredFileSystemDiff(IFileSystemDiff wrappedDiff, IEqualityComparer<IFile> fileComparer)
        {
            if (wrappedDiff == null)
            {
                throw new ArgumentNullException(nameof(wrappedDiff));
            }
            if (fileComparer == null)
            {
                throw new ArgumentNullException(nameof(fileComparer));
            }
            m_WrappedDiff = wrappedDiff;
            m_FileComparer = fileComparer;
        }


        public IFileSystemSnapshot FromSnapshot => m_WrappedDiff.FromSnapshot;

        public IFileSystemSnapshot ToSnapshot => m_WrappedDiff.ToSnapshot;

        public IEnumerable<IChange> Changes => FilterChanges(m_WrappedDiff.Changes);


        IEnumerable<IChange> FilterChanges(IEnumerable<IChange> changes)
        {
            return changes.Where(change => !m_FileComparer.Equals(change.FromFile, change.ToFile));
        }

    }
}