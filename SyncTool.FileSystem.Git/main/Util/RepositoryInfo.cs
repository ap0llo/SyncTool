// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfo
    {

        public Version SyncToolVersion { get; set; }


        public RepositoryInfo()
        {
            SyncToolVersion = Assembly.GetExecutingAssembly().GetName().Version;
        }



     
    }
}