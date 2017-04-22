using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Synchronization;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Implementation of <see cref="IFileSystemDiff"/> that filters out changes 
    /// based on a <see cref="IChangeFilter"/>
    /// </summary>
    class FilteredFileSystemDiff : IFileSystemDiff
    {
        readonly IFileSystemDiff m_WrappedDiff;
        readonly IChangeFilter m_Filter;


        public IFileSystemHistory History => m_WrappedDiff.History;

        public IFileSystemSnapshot FromSnapshot => m_WrappedDiff.FromSnapshot;

        public IFileSystemSnapshot ToSnapshot => m_WrappedDiff.ToSnapshot;
        

        public IEnumerable<IChangeList> ChangeLists => FilterChanges(m_WrappedDiff.ChangeLists);


        public FilteredFileSystemDiff(IFileSystemDiff wrappedDiff, IChangeFilter filter)
        {
            if (wrappedDiff == null)
            {
                throw new ArgumentNullException(nameof(wrappedDiff));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            m_WrappedDiff = wrappedDiff;
            m_Filter = filter;
        }
        
        IEnumerable<IChangeList> FilterChanges(IEnumerable<IChangeList> changes)
        {
            foreach (var changeList in changes)
            {
                if (m_Filter.IncludeInResult(changeList))
                {
                    yield return new FilteredChangeList(changeList, m_Filter);                    
                }
            }
        }
    }
}