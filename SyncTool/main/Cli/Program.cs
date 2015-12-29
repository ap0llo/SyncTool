// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using Ninject;
using SyncTool.Cli.DI;
using SyncTool.Cli.Framework;
using SyncTool.Git.DI;

namespace SyncTool.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var kernel = new StandardKernel(new CliModule(), new GitConfigurationModule(), new GitFileSystemModule(), new GitVersioningModule());
            return kernel.Get<Application>().Run(args);
        }
    }
}