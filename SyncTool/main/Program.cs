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
using SyncTool.FileSystem.Git.DI;
using SyncTool.FileSystem.Local;
using SyncTool.FileSystem.Versioning;
using SyncTool.FileSystem.Versioning.Git;
using SyncTool.FileSystem.Versioning.Git.DI;

namespace SyncTool
{
    class Program : IDisposable
    {
        static int Main(string[] args)
        {
            var kernel = new StandardKernel(new GitConfigurationModule(), new GitFileSystemModule(), new GitVersioningModule());
            using (var program = kernel.Get<Program>())
            {
                return program.Run(args);
            }
        }


        readonly ISyncGroupManager m_GroupManager;
        readonly IHistoryRepositoryManager m_HistoryRepositoryManager;


        public Program(ISyncGroupManager groupManager, IHistoryRepositoryManager historyRepositoryManager)
        {
            if (groupManager == null)
            {
                throw new ArgumentNullException(nameof(groupManager));
            }
            if (historyRepositoryManager == null)
            {
                throw new ArgumentNullException(nameof(historyRepositoryManager));
            }

            m_GroupManager = groupManager;
            m_HistoryRepositoryManager = historyRepositoryManager;
        }



        int Run(string[] args)
        {
            var parser = new Parser(opts =>
            {
                opts.CaseSensitive = false;
                opts.HelpWriter = Parser.Default.Settings.HelpWriter;               
            });

            var result = parser.ParseArguments<AddSyncGroupOptions, GetSyncGroupOptions, AddSyncFolderOptions, GetSnapshotOptions, AddSnapshotOptions>(args);            

            return result.MapResult(
                (GetSyncGroupOptions opts) =>
                {
                    var groupNames = String.IsNullOrEmpty(opts.Name)
                        ? m_GroupManager.SyncGroups
                        : m_GroupManager.SyncGroups.Where(g => g.Equals(opts.Name, StringComparison.InvariantCultureIgnoreCase));

                    Console.WriteLine();
                    foreach (var group in groupNames.Select(m_GroupManager.GetSyncGroup))
                    {
                        using (group)
                        {
                            PrintSyncGroup(group, " ");
                            Console.WriteLine();                            
                        }
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
                    using (var syncGroup = m_GroupManager.GetSyncGroup(opts.Group))
                    using (var historyRepository = m_HistoryRepositoryManager.GetHistoryRepository(opts.Group))
                    {
                        syncGroup.AddSyncFolder(new SyncFolder() { Name = opts.Name, Path = opts.Path });
                        historyRepository.CreateHistory(opts.Name);
                    }
                    return 0;
                },
                (GetSnapshotOptions opts) =>
                {
                    using (var group = m_GroupManager.GetSyncGroup(opts.Group))
                    using (var historyRepository = m_HistoryRepositoryManager.GetHistoryRepository(opts.Group))
                    {
                        PrintSyncFolder(group[opts.Folder], " ");

                        var history = historyRepository.GetHistory(opts.Folder);                           
                        PrintHistory(history, " \t");

                    }
                    return 0;
                },
                (AddSnapshotOptions opts) =>
                {
                    using (var group = m_GroupManager.GetSyncGroup(opts.Group))
                    using (var historyRepository = m_HistoryRepositoryManager.GetHistoryRepository(opts.Group))
                    {
                        var folder = group[opts.Folder];
                        var history = historyRepository.GetHistory(opts.Folder);                                                                      

                        var state = new LocalDirectory(null, folder.Path);

                       //TODO: Apply filter

                       history.CreateSnapshot(state);

                       return 0;
                    }                 
                },
                errs =>
                {                    
                    PrintError(" Unable to parse command line arguments");
                    return 1;
                }
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

        void PrintHistory(IFileSystemHistory history, string prefix)
        {
            if (history != null && history.Snapshots.Any())
            {
                foreach (var snapshot in history.Snapshots)
                {
                    Console.WriteLine($"{prefix}\t{snapshot.CreationTime}\t{snapshot.Id}");
                }
            }
            else
            {
                Console.WriteLine($"{prefix}No snapshots found");
            }
         
        }


        void PrintError(string error, string prefix = "")
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(prefix + error);
            Console.ForegroundColor = color;
        }



        public void Dispose()
        {
            m_GroupManager.Dispose();
        }
    }
}
