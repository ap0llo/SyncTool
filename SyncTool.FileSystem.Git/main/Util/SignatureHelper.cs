// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using LibGit2Sharp;

namespace SyncTool.FileSystem.Git
{
    public class SignatureHelper
    {      
        //TODO
        public static Signature NewSignature() => new Signature("SyncTool", "SyncTool@example.com", DateTimeOffset.Now);
    }
}