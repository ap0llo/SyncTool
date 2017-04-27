using System;
using SyncTool.Cli.Output;

namespace SyncTool.Cli.Commands
{
    public abstract class CommandBase
    {        
        public IOutputWriter OutputWriter { get; }


        protected CommandBase(IOutputWriter outputWriter)
        {
            OutputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
        }

    }
}