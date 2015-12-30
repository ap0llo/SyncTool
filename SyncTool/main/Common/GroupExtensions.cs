using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncTool.FileSystem.Versioning;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.Common
{
    public static class GroupExtensions
    {

        public static IHistoryService GetHistoryService(this IGroup group) => group.GetService<IHistoryService>();

        public static ISynchronizationStateService GetSynchronizationStateService(this IGroup group) => group.GetService<ISynchronizationStateService>();
    }
}
