// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem.Git
{
    [Serializable]
    public class GroupNotFoundException : Exception
    {
        public GroupNotFoundException(string name) : base($"The Group '{name}' could not be found")
        {
        }
    }
}