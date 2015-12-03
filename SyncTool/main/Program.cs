// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using CommandLine;
using Ninject;
using SyncTool.Cli;
using SyncTool.Configuration.Git.DI;
using SyncTool.Configuration.Model;

namespace SyncTool
{
    class Program
    {
        static int Main(string[] args)
        {
            using (var kernel = new StandardKernel(new GitConfigurationModule()))
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
            var result = CommandLine.Parser.Default.ParseArguments<AddSyncGroupOptions, GetSyncGroupOptions, AddSyncFolderOptions>(args);            

            return result.MapResult(
                (GetSyncGroupOptions opts) =>
                {
                    var groups = String.IsNullOrEmpty(opts.Name)
                        ? m_GroupManager.SyncGroups
                        : m_GroupManager.SyncGroups.Where(g => g.Name.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

                    Console.WriteLine();
                    foreach (var group in groups)
                    {                        
                        PrintSyncGroup(group);
                        Console.WriteLine();
                    }

                    return 0;
                },
                (AddSyncGroupOptions opts) =>
                {
                    m_GroupManager.AddSyncGroup(opts.Name);
                    return 0;
                },
                (AddSyncFolderOptions opts) =>
                {
                    var syncGroup = m_GroupManager[opts.Group];
                    syncGroup.AddSyncFolder(new SyncFolder() { Name = opts.Name, Path = opts.Path});
                    return 0;
                },
                errs => 1
                );
        }


        void PrintSyncGroup(ISyncGroup group, string prefix = "")
        {
            Console.WriteLine($"{prefix}SyncGroup '{group.Name}'");

            if (group.Folders.Any())
            {
                Console.WriteLine($"{prefix}\tFolders:");
                foreach (var folder in group.Folders)
                {
                    PrintSyncFolder(folder, $"{prefix}\t\t");
                }
            }
            else
            {
                Console.WriteLine(" \tNo folders in this sync group");
            }
        }


        void PrintSyncFolder(SyncFolder folder, string prefix = "")
        {
            Console.WriteLine($"{prefix}{folder.Name} --> {folder.Path}");
        }



    }
}
