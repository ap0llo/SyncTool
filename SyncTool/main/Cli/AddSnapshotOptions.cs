// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using CommandLine;

namespace SyncTool.Cli
{
    [Verb("Add-Snapshot")]
    public class AddSnapshotOptions
    {

        [Option(Required = true)]
        public string Group { get; set; }

        [Option(Required = true)]
        public string Folder { get; set; }

    }
}