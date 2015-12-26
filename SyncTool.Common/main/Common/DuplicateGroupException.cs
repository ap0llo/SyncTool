// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Common
{
    //TODO: Rename and move to different package
    [Serializable]
    public class DuplicateGroupException : GroupManagerException
    {
        public DuplicateGroupException(string name) : base($"A SyncGroup called '{name}' already exists")
        {
        }
    }
}