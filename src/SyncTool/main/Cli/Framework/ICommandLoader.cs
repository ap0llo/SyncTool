using System;

namespace SyncTool.Cli.Framework
{
    public interface ICommandLoader
    {
        CommandDescription[] GetCommands();   
    }
}