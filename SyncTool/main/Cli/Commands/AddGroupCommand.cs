// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using SyncTool.Cli.Framework;
using SyncTool.Cli.Output;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Cli.Commands
{
    [Verb("Add-Group")]
    public class AddGroupOptions
    {
        [Option('n', "name", Required = true)]
        public string Name { get; set; }
    }


    public class AddGroupCommand : CommandBase, ICommand<AddGroupOptions>
    {
        readonly IGroupManager m_GroupManager;

        public AddGroupCommand(IOutputWriter outputWriter, IGroupManager groupManager)  : base(outputWriter)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }
            m_GroupManager = groupManager;
        }


        public int Run(AddGroupOptions opts)
        {
            m_GroupManager.AddGroup(opts.Name);
            return 0;
        }

    }
}