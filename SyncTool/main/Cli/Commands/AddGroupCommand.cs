// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Security.Policy;
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

        [Option('a', "address", Required = true)]
        public string Address { get; set; }

        [Option("create", Required = false)]
        public bool Create { get; set; }
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
            if (opts.Create)
            {
                m_GroupManager.CreateGroup(opts.Name, opts.Address);
            }
            else
            {
                m_GroupManager.AddGroup(opts.Name, opts.Address);                
            }
            return 0;
        }

    }
}