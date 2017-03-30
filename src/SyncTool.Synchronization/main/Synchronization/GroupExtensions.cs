// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Synchronization
{
    public static class GroupExtensions
    {
        public static ISyncPointService GetSyncPointService(this IGroup group) => group.GetService<ISyncPointService>();

        public static ISyncActionService GetSyncActionService(this IGroup group) => group.GetService<ISyncActionService>();

        public static IConflictService GetSyncConflictService(this IGroup group) => group.GetService<IConflictService>();
    }
}