﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

namespace SyncTool.Cli.Framework
{
    public interface ICommand<T> where T : new ()
    {
        int Run(T opts);
    }
}