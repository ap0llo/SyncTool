using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Cli.Framework
{
    public class OptionsBase 
    {    
        [Option("debug", Required = false, Default = false)]
        public bool LaunchDebugger { get; set; }

    }
}
