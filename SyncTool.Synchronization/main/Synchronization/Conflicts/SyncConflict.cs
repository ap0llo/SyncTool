// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Synchronization.Conflicts
{
    public abstract class SyncConflict
    {
        
        public abstract string FilePath { get; }

        public string Description { get; set; }
        
    }
}