// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    [Serializable]
    public class GroupNotFoundException : GroupManagerException
    {
        public string GroupName { get; set; }

        public GroupNotFoundException(string groupName) : base($"The Group '{groupName}' could not be found")
        {
            this.GroupName = groupName;
        }
    }
}