// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using CommandLine;

namespace SyncTool.Cli
{
    [Verb("Add-Group")]
    public class AddSyncGroupOptions
    {
        [Option(Required = true)]
        public string Name { get; set; }
      
    }
}