// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Group")]
    public class AddGroupOptions
    {
        [Option(Required = true)]
        public string Name { get; set; }
    }


    public class AddGroupCommand : ICommand<AddGroupOptions>
    {
        readonly IGroupManager<IConfigurationGroup> m_ConfigurationGroupManager;

        public AddGroupCommand(IGroupManager<IConfigurationGroup> configurationGroupManager)
        {
            m_ConfigurationGroupManager = configurationGroupManager;
        }


        public int Run(AddGroupOptions opts)
        {
            m_ConfigurationGroupManager.AddGroup(opts.Name);
            return 0;
        }

    }
}