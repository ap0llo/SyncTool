// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;

namespace SyncTool.FileSystem.Versioning
{
    public static class GroupExtensions
    {

        public static IHistoryService GetHistoryService(this IGroup group) => group.GetService<IHistoryService>();

       
    }
}
