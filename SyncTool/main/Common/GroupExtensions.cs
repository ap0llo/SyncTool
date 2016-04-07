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
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Common
{
    public static class GroupExtensions
    {

        public static IHistoryService GetHistoryService(this IGroup group) => group.GetService<IHistoryService>();

        public static ISyncPointService GetSyncPointService(this IGroup group) => group.GetService<ISyncPointService>();

        public static ISyncActionService GetSyncActionService(this IGroup group) => group.GetService<ISyncActionService>();

        public static IConflictService GetSyncConflictService(this IGroup group) => group.GetService<IConflictService>();
    }
}
