﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

namespace SyncTool.FileSystem
{
    public interface IChange
    {
        ChangeType Type { get; }

        IFile FromFile { get; }

        IFile ToFile { get; }
    }
}