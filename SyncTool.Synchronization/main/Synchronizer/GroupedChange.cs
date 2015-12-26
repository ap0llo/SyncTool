// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    class GroupedChange
    {

        public string FilePath => LeftChange?.Path ?? RightChange.Path;

        public IChange LeftChange { get; }

        public IChange RightChange { get; }


        public GroupedChange(IChange leftChange, IChange rightChange)
        {
            if (leftChange == null && rightChange == null)
            {
                throw new ArgumentNullException($"{nameof(leftChange)} and {nameof(rightChange)} can't both be null");
            }

            if (leftChange != null && rightChange != null)
            {
                if (!StringComparer.InvariantCultureIgnoreCase.Equals(rightChange.Path, leftChange.Path))
                {
                    throw new ArgumentException($"Paths of {nameof(leftChange)} and {nameof(rightChange)} do not match");
                }
            }
            LeftChange = leftChange;
            RightChange = rightChange;
        }
             
    }
}