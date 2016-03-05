// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Reflection;

namespace SyncTool.Git.Common
{
    public class RepositoryInfo
    {

        public Version SyncToolVersion { get; set; } = Assembly.GetExecutingAssembly().GetName().Version;

    }
}