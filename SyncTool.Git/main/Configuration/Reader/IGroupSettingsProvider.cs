// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System.Collections.Generic;
using SyncTool.Git.Configuration.Model;

namespace SyncTool.Git.Configuration.Reader
{
    public interface IGroupSettingsProvider
    {
        IEnumerable<GroupSettings> GetGroupSettings();

        void SaveGroupSettings(IEnumerable<GroupSettings> settings);
    }
}