// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.Configuration
{
    public class DuplicateSyncFolderException : ConfigurationException
    {


        public DuplicateSyncFolderException(string name) : base($"A SyncFolder named '{name}' already exists")
        {
            
        }

    }
}