using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using Xunit;
using NodaTime;

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
                ""type"" : ""Added"",
                ""id"" : ""A7226A4D-4BE8-4B10-B378-BEF72A29FD24"",
                ""Target"": ""target"",
                ""State"" : ""Active"",
                ""SyncPointId"" : 23,
                ""ToVersion"": {
                  ""Path"": ""/dir1/file1"",
                  ""LastWriteTime"": ""2015-12-27T17:02:17.8666998+01:00"",
                  ""Length"": 23                
              }
            }");

        readonly SyncActionSerializer m_Instance;

        public SyncActionSerializerTest()
        {
            m_Instance = new SyncActionSerializer();
        }



        [Fact]
        public void Deserialize_missing_id_causes_SerializationException()
        {
            var value = (JObject) s_ValidJson.DeepClone();
            value.Remove("id");
                   
            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)                
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact]
        public void Deserialize_missing_state_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson.DeepClone();
            value.Remove("State");

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact]
        public void Deserialize_missing_SyncPointId_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson.DeepClone();
            value.Remove("SyncPointId");

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("name", s_ValidJson["name"]),
                new JProperty("value", value)
                );

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact]
        public void Deserialize_invalid_id_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson.DeepClone();
            value.Remove("id");
            value.Add(new JProperty("id", "This is not a Guid"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact]
        public void Deserialize_invalid_state_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson.DeepClone();
            value.Remove("State");
            value.Add(new JProperty("State", "This is not a SyncActionState"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }

        [Fact]
        public void Deserialize_invalid_SyncPointId_causes_SerializationException()
        {
            var value = (JObject)s_ValidJson.DeepClone();
            value.Remove("SyncPointId");
            value.Add(new JProperty("SyncPointId", "This is not an Integer"));

            // create new JObject without the id property
            var jObject = new JObject(
                new JProperty("value", value),
                new JProperty("name", s_ValidJson["name"]));

            Assert.Throws<SerializationException>(() => m_Instance.Deserialize(jObject.ToString()));
        }


        [Fact]
        public void SyncAction_Roundtrip_Added()
        {
            var fileReference = new FileReference("/file1", SystemClock.Instance.GetCurrentInstant(), 23);
            
            var expected = new SyncAction(ChangeType.Added, null, fileReference, Guid.Parse("A7226A4D-4BE8-4B10-B378-BEF72A29FD24"), "targetName", SyncActionState.Queued, 42);
            var json = m_Instance.Serialize(expected);
            var actual = m_Instance.Deserialize(json);

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.FromVersion, actual.FromVersion);
            Assert.Equal(expected.ToVersion, actual.ToVersion);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact]
        public void SyncAction_Roundtrip_Deleted()
        {
            var fileReference = new FileReference("/file1", SystemClock.Instance.GetCurrentInstant(), 23);

            var expected = new SyncAction(ChangeType.Deleted, fileReference, null, Guid.NewGuid(), "targetName", SyncActionState.Active,23);
            var actual = m_Instance.Deserialize(m_Instance.Serialize(expected));

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.ToVersion, actual.ToVersion);
            Assert.Equal(expected.FromVersion, actual.FromVersion);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact]
        public void SyncAction_Roundtrip_Modified()
        {
            var lastWriteTime = SystemClock.Instance.GetCurrentInstant();

            var oldVersion = new FileReference("/file1", lastWriteTime, 23);
            var newVersion = new FileReference("/file1", lastWriteTime + Duration.FromDays(1), 23 * 2);            
            
            var expected = new SyncAction(ChangeType.Modified, oldVersion, newVersion, Guid.NewGuid(), Guid.NewGuid().ToString(), SyncActionState.Completed, 42);
            var actual = m_Instance.Deserialize(m_Instance.Serialize(expected));

            Assert.NotNull(actual);
            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Target, actual.Target);
            Assert.Equal(expected.FromVersion, actual.FromVersion);
            Assert.Equal(expected.ToVersion, actual.ToVersion);
            Assert.Equal(expected.State, actual.State);
            Assert.Equal(expected.SyncPointId, actual.SyncPointId);
        }

        [Fact]
        public void State_property_is_serialized_as_string()
        {
            var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
            var oldVersion = new FileReference("/file1", lastWriteTime, 23);
            var newVersion = new FileReference("/file1", lastWriteTime + Duration.FromDays(1), 23 * 2);

            var syncAction = SyncAction.CreateReplaceFileSyncAction(Guid.NewGuid().ToString(), SyncActionState.Completed, 1, oldVersion, newVersion);

            var serialized = JObject.Parse(m_Instance.Serialize(syncAction));

            Assert.Equal(syncAction.State.ToString(), serialized["State"].ToString());
        }


        [Fact]
        public void Type_property_is_serialized_as_string()
        {
            var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
            var oldVersion = new FileReference("/file1", lastWriteTime, 23);
            var newVersion = new FileReference("/file1", lastWriteTime + Duration.FromDays(1), 23 * 2);

            var syncAction = SyncAction.CreateReplaceFileSyncAction(Guid.NewGuid().ToString(), SyncActionState.Completed, 1, oldVersion, newVersion);

            var serialized = JObject.Parse(m_Instance.Serialize(syncAction));

            Assert.Equal(syncAction.Type.ToString(), serialized["Type"].ToString());
        }

    }
}