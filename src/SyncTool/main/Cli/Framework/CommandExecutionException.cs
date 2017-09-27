using System;

namespace SyncTool.Cli.Framework
{
    sealed class CommandExecutionException : Exception
    {
        public CommandExecutionException(Exception innerException) : base("", innerException)
        {
        }
    }
}
