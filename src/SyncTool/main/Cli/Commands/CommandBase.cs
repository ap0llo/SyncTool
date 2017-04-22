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