// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SyncTool.Configuration.Model;

namespace SyncTool.Configuration.Reader
{
    public class JsonSyncFolderReader : ISyncFolderReader
    {

        readonly JsonSerializer m_Serializer = new JsonSerializer();


        public SyncFolder ReadSyncFolder(Stream stream)
        {
            var jsonReader = new JsonTextReader(new StreamReader(stream));
            return m_Serializer.Deserialize<SyncFolder>(jsonReader);                
        }

    }
}