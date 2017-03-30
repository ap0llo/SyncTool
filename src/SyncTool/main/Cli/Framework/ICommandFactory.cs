// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

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