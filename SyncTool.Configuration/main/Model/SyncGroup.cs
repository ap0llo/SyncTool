// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Configuration.Model
{
    public class SyncGroup
    {

        IEnumerable<SyncFolder> m_Folders; 



        public string Name { get; set; }

        public string MasterRepositoryPath { get; set; }
        
        public string LocalRepositoryPath { get; set; }

        public IEnumerable<SyncFolder> Folders
        {
            get { return m_Folders ?? Enumerable.Empty<SyncFolder>(); }
            set { m_Folders = value; }
        }
    
    }
}