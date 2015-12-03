// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using CommandLine;

namespace SyncTool.Cli
{
    [Verb("Add-SyncGroup")]
    public class AddSyncGroupOptions
    {
        [Option(Required = true)]
        public string Name { get; set; }

        [Option(Required = true)]
        public string LocalPath { get; set; }

        [Option(Required = false)]
        public string RemotePath { get; set; }
    }
}