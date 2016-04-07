﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Synchronization.SyncActions
{
    /// <summary>
    /// Tests for <see cref="SyncActionSerializer"/>
    /// </summary>
    public class SyncActionSerializerTest
    {
        static readonly JObject s_ValidJson = JObject.Parse(
            @"
            {
              ""name"": ""AddFileSyncAction"",
              ""value"": {
                ""id"" : ""A7226A4D-4BE8-4B10-B378-BEF72A29FD24"",
                ""Target"": ""target"",
                ""State"" : ""Active"",
                ""SyncPointId"" : 23,
                ""NewFile"": {
                  ""Path"": ""/dir1/file1"",
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



        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing id causes SerializationException")]
        public void Deserialize_missing_id_causes_SerializationException()
        {
            var value = (JObject) s_ValidJson["value"].DeepClone();
            value.Remove("id");
                   
            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)                
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing state causes SerializationException")]
        public void Deserialize_missing_state_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson["value"].DeepClone();
            value.Remove("State");

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing SyncPointId causes SerializationException")]
        public void Deserialize_missing_SyncPointId_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson["value"].DeepClone();
            value.Remove("SyncPointId");

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Invalid id causes SerializationException")]
        public void Deserialize_invalid_id_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson["value"].DeepClone();
            value.Remove("id");
            value.Add(new JProperty("id", "This is not a Guid"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }        

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Invalid state causes SerializationException")]
        public void Deserialize_invalid_state_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson["value"].DeepClone();
            value.Remove("State");
            value.Add(new JProperty("State", "This is not a SyncActionState"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Invalid SyncPointId causes SerializationException")]
        public void Deserialize_invalid_SyncPointId_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson["value"].DeepClone();
            value.Remove("SyncPointId");
            value.Add(new JProperty("SyncPointId", "This is not an Integer"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing name causes SerializationException")]   
        public void Deserialize_missing_name_causes_SerializationException()
        {          
            // create new JObject without the name property
            var jObject = new JObject(
                new JProperty("value", s_ValidJson["value"]), 
                new JProperty("id", s_ValidJson["id"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ".Deserialize(): Missing value causes SerializationException")]  
        public void Deserialize_missing_value_causes_SerializationException()
        {
            // create new JObject without the value property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("id", s_ValidJson["id"]));
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
            var fileReference = new FileReference("/file1", DateTime.Now, 23);
            
            var expected = new AddFileSyncAction(Guid.Parse("A7226A4D-4BE8-4B10-B378-BEF72A29FD24"), "targetName", SyncActionState.Queued, 42, fileReference);
            var actual = (AddFileSyncAction) m_Instance.Deserialize(m_Instance.Serialize(expected));

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.NewFile, actual.NewFile);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "RemoveFileSyncAction: Roundtrip")]
        public void RemoveFileSyncAction_Roundtrip()
        {
            var fileReference = new FileReference("/file1", DateTime.Now, 23);

            var expected = new RemoveFileSyncAction(Guid.NewGuid(), "targetName", SyncActionState.Active,23, fileReference);
            var actual = (RemoveFileSyncAction) m_Instance.Deserialize(m_Instance.Serialize(expected));

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.RemovedFile, actual.RemovedFile);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + "ReplaceFileSyncAction: Roundtrip")]
        public void ReplaceFileSyncAction_Roundtrip()
        {
            var lastWriteTime = DateTime.Now;

            var oldVersion = new FileReference("/file1", lastWriteTime, 23);
            var newVersion = new FileReference("/file1", lastWriteTime.AddDays(1), 23 * 2);            
            
            var expected = new ReplaceFileSyncAction(Guid.NewGuid(), Guid.NewGuid().ToString(), SyncActionState.Completed, 42,  oldVersion, newVersion);
            var actual = (ReplaceFileSyncAction) m_Instance.Deserialize(m_Instance.Serialize(expected));

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.OldVersion, actual.OldVersion);
            Assert.Equal(expected.NewVersion, actual.NewVersion);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact(DisplayName = nameof(SyncActionSerializer) + ": State property is serialized as string")]
        public void State_property_is_serialized_as_string()
        {
            var lastWriteTime = DateTime.Now;
            var oldVersion = new FileReference("/file1", lastWriteTime, 23);
            var newVersion = new FileReference("/file1", lastWriteTime.AddDays(1), 23 * 2);

            var syncAction = new ReplaceFileSyncAction(Guid.NewGuid(), Guid.NewGuid().ToString(), SyncActionState.Completed, 1, oldVersion, newVersion);

            var serialized = JObject.Parse(m_Instance.Serialize(syncAction));

            Assert.Equal(syncAction.State.ToString(), serialized["value"]["State"].ToString());

        }


    }
}