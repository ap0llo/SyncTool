// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using CommandLine;

namespace SyncTool.Cli
{
    [Verb("Add-SyncFolder"), ]
    public class AddSyncFolderOptions
    {
        [Option(Required = true)]
        public string Group { get; set; }

        [Option(Required = true)]
        public string Name { get; set; }
        
        [Option(Required = true)]
        public string Path { get; set; }
    }
}