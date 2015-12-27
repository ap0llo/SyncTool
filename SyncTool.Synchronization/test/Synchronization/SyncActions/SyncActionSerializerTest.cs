// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization.SyncActions
{
    public class SyncActionSerializerTest
    {
        static readonly JObject s_ValidJson = JObject.Parse(
            @"
            {
              ""name"": ""AddFileSyncAction"",
              ""value"": {
                ""Target"": 1,
                ""NewFile"": {
                  ""Path"": ""dir1/file1"",
                  ""LastWriteTime"": ""2015-12-27T17:02:17.8666998+01:00"",
                  ""Length"": 23
                }
              }
            }");

        readonly SyncActionSerializer m_Instance;

        public SyncActionSerializerTest()
        {
            m_Instance = new SyncActionSerializer();
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing name causes SerializationException")]   
        public void Deserialize_missing_name_causes_SerializationException()
        {          
            var jObject = new JObject(new JProperty("value", s_ValidJson["value"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing value causes SerializationException")]  
        public void Deserialize_missing_value_causes_SerializationException()
        {
            var jObject = new JObject(new JProperty("name", s_ValidJson["name"]));
            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Unknown name causes SerializationException")]   
        public void Deserialize_unknown_name_causes_SerializationException()
        {
            var jObject = new JObject(new JProperty("name", "SomeNonsenseValue"));
            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "AddFileSyncAction: Parse sample json")]
        public void AddFileSyncAction_Parse_sample_json()
        {
            var action = m_Instance.Deserialize(s_ValidJson.ToString());

            Assert.NotNull(action);
            Assert.IsType<AddFileSyncAction>(action);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "AddFileSyncAction: Roundtrip")]
        public void AddFileSyncAction_Roundtrip()
        {            
            var file = new File(new NullDirectory("dir1", "dir1"),"file1" )
            {
                LastWriteTime = DateTime.Now,
                Length = 23
            };
            
            var expected = new AddFileSyncAction(SyncParticipant.Right, file);
            var actual = m_Instance.Deserialize(m_Instance.Serialize(expected)) as AddFileSyncAction;

            Assert.NotNull(actual);
            Assert.Equal(expected.Target, actual.Target);
            FileSystemAssert.FileEqual(expected.NewFile, actual.NewFile);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "RemoveFileSyncAction: Roundtrip")]
        public void RemoveFileSyncAction_Roundtrip()
        {
            var file = new File(new NullDirectory("dir1", "dir1"), "file1")
            {
                LastWriteTime = DateTime.Now,
                Length = 23
            };

            var expected = new RemoveFileSyncAction(SyncParticipant.Right, file);
            var actual = m_Instance.Deserialize(m_Instance.Serialize(expected)) as RemoveFileSyncAction;

            Assert.NotNull(actual);
            Assert.Equal(expected.Target, actual.Target);
            FileSystemAssert.FileEqual(expected.RemovedFile, actual.RemovedFile);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "ReplaceFileSyncAction: Roundtrip")]
        public void ReplaceFileSyncAction_Roundtrip()
        {
            var lastWriteTime = DateTime.Now;
            var oldVersion = new File(new NullDirectory("dir1", "dir1"), "file1")
            {
                LastWriteTime = lastWriteTime,
                Length = 23
            };

            var newVersion = new File(new NullDirectory("dir1", "dir1"), "file1")
            {
                LastWriteTime = lastWriteTime.AddDays(1),
                Length = 23 * 2 
            };

            var expected = new ReplaceFileSyncAction(SyncParticipant.Right, oldVersion, newVersion);
            var actual = m_Instance.Deserialize(m_Instance.Serialize(expected)) as ReplaceFileSyncAction;

            Assert.NotNull(actual);
            Assert.Equal(expected.Target, actual.Target);
            FileSystemAssert.FileEqual(expected.OldVersion, actual.OldVersion);
            FileSystemAssert.FileEqual(expected.NewVersion, actual.NewVersion);
        }

    }
}