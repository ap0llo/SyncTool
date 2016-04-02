// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.State;

namespace SyncTool.Common
{
    public static class GroupExtensions
    {

        public static IHistoryService GetHistoryService(this IGroup group) => group.GetService<IHistoryService>();

        public static ISynchronizationStateService GetSynchronizationStateService(this IGroup group) => group.GetService<ISynchronizationStateService>();
    }
}
