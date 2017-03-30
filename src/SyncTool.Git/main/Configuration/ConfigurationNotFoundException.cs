// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Git.Configuration
{
    [Serializable]
    public class ConfigurationNotFoundException : Exception
    {

        public ConfigurationNotFoundException(string message) : base(message)
        {
            
        }

    }
}