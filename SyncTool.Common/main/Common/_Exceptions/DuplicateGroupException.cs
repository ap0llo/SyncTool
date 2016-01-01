// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{


    /// <summary>
    /// Indicates that a group could not be added because a group with the specified groupName already exists
    /// </summary>
    [Serializable]
    public class DuplicateGroupException : GroupManagerException
    {
        public string GroupName { get; }

        public DuplicateGroupException(string groupName) : base($"A Group named '{groupName}' already exists")
        {
            this.GroupName = groupName;
        }
    }
}