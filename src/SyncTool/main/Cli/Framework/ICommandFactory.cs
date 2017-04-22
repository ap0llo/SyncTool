using System;

namespace SyncTool.Cli.Framework
{
    public interface ICommandFactory
    {
        /// <summary>
        /// Creates a command instance of the specified type
        /// </summary>               
        object CreateCommandInstance(Type commandType);
    }
}