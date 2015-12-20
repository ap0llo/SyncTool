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

        public string FilePath => GlobalChange?.Path ?? LocalChange.Path;

        public IChange GlobalChange { get; }

        public IChange LocalChange { get; }


        public GroupedChange(IChange globalChange, IChange localChange)
        {
            if (globalChange == null && localChange == null)
            {
                throw new ArgumentNullException($"{nameof(globalChange)} and {nameof(localChange)} can't both be null");
            }

            if (globalChange != null && localChange != null)
            {
                if (!StringComparer.InvariantCultureIgnoreCase.Equals(localChange.Path, globalChange.Path))
                {
                    throw new ArgumentException($"Paths of {nameof(globalChange)} and {nameof(localChange)} do not match");
                }
            }
            GlobalChange = globalChange;
            LocalChange = localChange;
        }
             
    }
}