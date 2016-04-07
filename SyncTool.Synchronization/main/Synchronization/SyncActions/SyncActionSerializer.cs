// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public class SyncActionSerializer 
    {
        const string s_Name = "name";
        const string s_Value = "value";
                


        public string Serialize(SyncAction action) => SerializeToJObject(action).ToString();
      

        public void Serialize(SyncAction action, System.IO.Stream writeTo)
        {
            var writer = new JsonTextWriter(new StreamWriter(writeTo));
            SerializeToJObject(action).WriteTo(writer);
            writer.Flush();
        }

        public SyncAction Deserialize(Stream stream)
        {
            var jsonReader = new JsonTextReader(new System.IO.StreamReader(stream));
            var json = JObject.Load(jsonReader);
            return Deserialize(json);
        }

        public SyncAction Deserialize(string jsonString)
        {            
            var json = JObject.Parse(jsonString);
            return Deserialize(json);
        }


        void Serialize(ReplaceFileSyncAction action, JObject jsonObject)
        {            
            var dto = new ReplaceFileSyncActionDto(action);
            var value = JObject.Parse(JsonConvert.SerializeObject(dto));
            jsonObject.Add(s_Value, value);
        }

        void Serialize(AddFileSyncAction action, JObject jsonObject)
        {
            var dto = new AddFileSyncActionDto(action);
            var value = JObject.Parse(JsonConvert.SerializeObject(dto));            
            jsonObject.Add(s_Value, value);
        }

        void Serialize(RemoveFileSyncAction action, JObject jsonObject)
        {
            var dto = new RemoveFileSyncActionDto(action);
            var value = JObject.Parse(JsonConvert.SerializeObject(dto));
            jsonObject.Add(s_Value, value);
        }


        JObject SerializeToJObject(SyncAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var jObject = new JObject()
            {
                new JProperty(s_Name, action.GetType().Name)
            };

            Serialize((dynamic) action, jObject);            

            return jObject;
        }

        SyncAction Deserialize(JObject json)
        {
            var name = GetPropertyValue(json, s_Name, JTokenType.String).ToString();
            var value = GetObjectProperty(json, s_Value);

            try
            {
                switch (name)
                {
                    case nameof(ReplaceFileSyncAction):
                        return DeserializeReplaceFileSyncAction(value);

                    case nameof(AddFileSyncAction):
                        return DeserializeAddFileSyncAction(value);

                    case nameof(RemoveFileSyncAction):
                        return DeserializeRemoveFileSyncAction(value);

                    default:
                        throw new SerializationException($"Error deserializing json as SyncAction. Unknown action name {name}");
                }
            }
            catch (JsonSerializationException ex)
            {
                throw new SerializationException("Error deserializing json as SyncAction", ex);
            }
        }


        AddFileSyncAction DeserializeAddFileSyncAction(JObject json)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<AddFileSyncActionDto>(json.ToString());
                return new AddFileSyncAction(dto.Id, dto.Target, dto.State, dto.SyncPointId, DeserializeFileReference(dto.NewFile));
            }
            catch (JsonReaderException ex)
            {
                throw new SerializationException($"Could not deserialize " + nameof(AddFileSyncAction), ex);
            }
        }

        RemoveFileSyncAction DeserializeRemoveFileSyncAction(JObject json)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<RemoveFileSyncActionDto>(json.ToString());
                return new RemoveFileSyncAction(dto.Id, dto.Target, dto.State, dto.SyncPointId, DeserializeFileReference(dto.RemovedFile));
            }
            catch (JsonReaderException ex)
            {
                throw new SerializationException($"Could not deserialize " + nameof(RemoveFileSyncAction), ex);
            }
        }

        ReplaceFileSyncAction DeserializeReplaceFileSyncAction(JObject json)
        {
            try
            {
                var dto = JsonConvert.DeserializeObject<ReplaceFileSyncActionDto>(json.ToString());
                return new ReplaceFileSyncAction(dto.Id, dto.Target, dto.State, dto.SyncPointId, DeserializeFileReference(dto.OldVersion), DeserializeFileReference(dto.NewVersion));
            }
            catch (JsonReaderException ex)
            {
                throw new SerializationException($"Could not deserialize " + nameof(ReplaceFileSyncActionDto), ex);
            }
        }

        JObject GetObjectProperty(JObject parent, string name)
        {
            JObject child = GetPropertyValue(parent, name, JTokenType.Object) as JObject;            
            if (child == null)
            {
                throw new SerializationException($"Error deserializing SyncAction from json. Child object '{name}' is missing");
            }
            return child;
        }
        
        JToken GetPropertyValue(JObject jObject, string propertyName, JTokenType type)
        {
            var property = jObject[propertyName];
            if (property == null || property.Type != type)
            {
                throw new SerializationException($"Error deserializing SyncAction from json. Property '{propertyName}' is missing or of wrong type");
            }

            return property;
        }
        

        IFileReference DeserializeFileReference(FileReferenceDto dto)
        {   
            return new FileReference(dto.Path, dto.LastWriteTime, dto.Length);
        }



        private abstract class SyncActionDto
        {
            [JsonRequired]
            public string Target { get; set; }

            [JsonRequired]
            public Guid Id { get; set; }

            [JsonRequired, JsonConverter(typeof(StringEnumConverter))]
            public SyncActionState State { get; set; }

            [JsonRequired]
            public int SyncPointId { get; set; }

            protected SyncActionDto()
            {
                
            }

            protected SyncActionDto(SyncAction action)
            {
                Target = action.Target;
                Id = action.Id;
                State = action.State;
                SyncPointId = action.SyncPointId;
            }
        }

        private class AddFileSyncActionDto : SyncActionDto
        {
            [JsonRequired]
            public FileReferenceDto NewFile { get; set; }

            public AddFileSyncActionDto()
            {
                
            }

            public AddFileSyncActionDto(AddFileSyncAction action) : base(action)
            {
                NewFile = new FileReferenceDto(action.NewFile);   
            }
        }

        private class RemoveFileSyncActionDto : SyncActionDto
        {
            [JsonRequired]
            public FileReferenceDto RemovedFile { get; set; }

            public RemoveFileSyncActionDto()
            {
                
            }

            public RemoveFileSyncActionDto(RemoveFileSyncAction action) : base(action)
            {
                RemovedFile = new FileReferenceDto(action.RemovedFile);
            }
        }

        private class ReplaceFileSyncActionDto : SyncActionDto
        {
            public ReplaceFileSyncActionDto()
            {
                
            }

            public ReplaceFileSyncActionDto(ReplaceFileSyncAction action) : base(action)
            {
                OldVersion = new FileReferenceDto(action.OldVersion);
                NewVersion = new FileReferenceDto(action.NewVersion);
            }

            [JsonRequired]
            public FileReferenceDto OldVersion { get; set; }

            [JsonRequired]
            public FileReferenceDto NewVersion { get; set; }
        }

        private class FileReferenceDto 
        {
            public string Path { get; set; }

            public DateTime? LastWriteTime { get; set; }

            public long? Length { get; set; }


            public FileReferenceDto()
            {
                
            }

            public FileReferenceDto(IFileReference reference)
            {
                Path = reference.Path;
                LastWriteTime = reference.LastWriteTime;
                Length = reference.Length;
            }
        }

       

    }
}