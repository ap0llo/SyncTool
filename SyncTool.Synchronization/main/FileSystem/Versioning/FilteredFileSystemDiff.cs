// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Implementation of <see cref="IFileSystemDiff"/> that filters out changes if the file 
    /// before and after the change is considered equal according to the specified equality comparer
    /// </summary>
    class FilteredFileSystemDiff : IFileSystemDiff
    {
        readonly IFileSystemDiff m_WrappedDiff;
        readonly IEqualityComparer<IFile> m_FileComparer;



        public IFileSystemHistory History => m_WrappedDiff.History;

        public IFileSystemSnapshot FromSnapshot => m_WrappedDiff.FromSnapshot;

        public IFileSystemSnapshot ToSnapshot => m_WrappedDiff.ToSnapshot;

        public IEnumerable<IChange> Changes => FilterChanges(m_WrappedDiff.Changes);

        public IEnumerable<IChangeList> ChangeLists {  get { throw new NotImplementedException();} } 

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


        IEnumerable<IChange> FilterChanges(IEnumerable<IChange> changes)
        {
            return changes.Where(change => !m_FileComparer.Equals(change.FromFile, change.ToFile));
        }

    }
}