// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using Ninject;
using SyncTool.Cli;
using SyncTool.Configuration.Model;

namespace SyncTool
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var kernel = new StandardKernel())
            {
                var program = kernel.Get<Program>();
                return program.Run(args);
            }
        }


        readonly ISyncGroupManager m_GroupManager;


        public Program(ISyncGroupManager groupManager)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }

            m_GroupManager = groupManager;
        }



        int Run(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<AddSyncGroupOptions, GetSyncGroupOptions>(args);

            return result.MapResult(
                (GetSyncGroupOptions opts) =>
                {
                    var groups = String.IsNullOrEmpty(opts.Name)
                        ? m_GroupManager.SyncGroups
                        : m_GroupManager.SyncGroups.Where(g => g.Name.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

                    foreach (var group in groups)
                    {                        
                        Console.WriteLine("SyncGroup " + group.Name);
                        Console.WriteLine("\tFolders:");
                        foreach (var folder in group.Folders)
                        {
                            Console.WriteLine("\t\t" + folder.Name);
                        }
                    }

                    return 0;
                },
                (AddSyncGroupOptions opts) => 1,
                errs => 1
                );
        }



    }
}
