using System;
using System.Linq;
using System.Collections.Generic;
using SyncTool.Common.Groups;
using SyncTool.FileSystem;


namespace SyncTool.Synchronization
{
    public class Synchronizer : ISynchronizer
    {        
        readonly IEqualityComparer<IFileReference> m_FileReferenceComparer;
               
        
        public Synchronizer(IEqualityComparer<IFileReference> fileReferenceComparer)
        {
            m_FileReferenceComparer = fileReferenceComparer ?? throw new ArgumentNullException(nameof(fileReferenceComparer));            
        }
        

        public void Synchronize(IGroup group)
        {
                        
        }
        

        
        

    }
}