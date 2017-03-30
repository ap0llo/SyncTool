// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using SyncTool.Common;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration
{
    public static class GroupExtensions
    {
        public static IConfigurationService GetConfigurationService(this IGroup group) => group.GetService<IConfigurationService>();
    }
}