// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using Ninject;
using SyncTool.Cli.Output;

namespace SyncTool.Cli.Commands
{
    public abstract class CommandBase
    {        
        public IOutputWriter OutputWriter { get; }


        protected CommandBase(IOutputWriter outputWriter)
        {
            if (outputWriter == null)
            {
                throw new ArgumentNullException(nameof(outputWriter));
            }
            this.OutputWriter = outputWriter;
        }

    }
}