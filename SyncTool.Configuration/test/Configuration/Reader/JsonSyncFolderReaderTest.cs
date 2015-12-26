// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using SyncTool.Common;
using SyncTool.Configuration.Model;
using SyncTool.Configuration.Reader;
using Xunit;

namespace SyncTool.Configuration.Reader
{
    public class JsonSyncFolderReaderTest
    {

        readonly JsonSyncFolderReader m_Instance = new JsonSyncFolderReader();


        [Fact(DisplayName = nameof(JsonSyncFolderReader) + ".ReadSyncFolder() successfully reads valid json stream")]
        public void ReadSyncFolder_successfully_reads_valid_json_stream()
        {


            var json1 = @"
                            { 
                                ""name"" : ""foo"", 
                                ""path"" : ""bar""                                 
                            }
                         ";

            var json2 = @"
                            {   
                                ""name"" : ""foo2"", 
                                ""path"" : ""bar2"",                                 
                                ""filter"" : { ""type"" : ""microscopeQuery"" , ""query"" : ""test"" }
                            }                
                        ";


            SyncFolder syncFolder1;
            SyncFolder syncFolder2;
            using (var stream = json1.ToStream())
            {
                syncFolder1 = m_Instance.ReadSyncFolder(stream);
            }
            using (var stream = json2.ToStream())
            {
                syncFolder2 = m_Instance.ReadSyncFolder(stream);
            }



            Assert.Equal("foo", syncFolder1.Name);
            Assert.Equal("bar", syncFolder1.Path);
            Assert.Null(syncFolder1.Filter);

            Assert.Equal("foo2", syncFolder2.Name);
            Assert.Equal("bar2", syncFolder2.Path);
            Assert.NotNull(syncFolder2.Filter);
            Assert.Equal(FileSystemFilterType.MicroscopeQuery, syncFolder2.Filter.Type);
            Assert.Equal("test", syncFolder2.Filter.Query);

        }

    }
}