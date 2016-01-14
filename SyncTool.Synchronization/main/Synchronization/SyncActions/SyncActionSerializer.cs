// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;
using File = SyncTool.FileSystem.File;

namespace SyncTool.Synchronization.SyncActions
{
    public class SyncActionSerializer : ISyncActionVisitor<JObject>
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

        public SyncAction Deserialize(System.IO.Stream stream)
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


        public void Visit(ReplaceFileSyncAction action, JObject jsonObject)
        {
            var dto = new ReplaceFileSyncActionDto(action);
            var value = JObject.Parse(JsonConvert.SerializeObject(dto));
            jsonObject.Add(s_Value, value);
        }

        public void Visit(AddFileSyncAction action, JObject jsonObject)
        {            
            var dto = new AddFileSyncActionDto(action);
            var value = JObject.Parse(JsonConvert.SerializeObject(dto));            
            jsonObject.Add(s_Value, value);
        }

        public void Visit(RemoveFileSyncAction action, JObject jsonObject)
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

            action.Accept(this, jObject);

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
            var dto = JsonConvert.DeserializeObject<AddFileSyncActionDto>(json.ToString());
            return new AddFileSyncAction(dto.Id, dto.Target, DeserializeFile(dto.NewFile));
        }

        RemoveFileSyncAction DeserializeRemoveFileSyncAction(JObject json)
        {
            var dto = JsonConvert.DeserializeObject<RemoveFileSyncActionDto>(json.ToString());
            return new RemoveFileSyncAction(dto.Id, dto.Target, DeserializeFile(dto.RemovedFile));
            
        }

        ReplaceFileSyncAction DeserializeReplaceFileSyncAction(JObject json)
        {
            var dto = JsonConvert.DeserializeObject<ReplaceFileSyncActionDto>(json.ToString());
            return new ReplaceFileSyncAction(dto.Id, dto.Target, DeserializeFile(dto.OldVersion), DeserializeFile(dto.NewVersion));            
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
        

        IFile DeserializeFile(FileDto dto)
        {           
            var pathParts = dto.Path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var fileName = pathParts.Last();
            pathParts = pathParts.Take(pathParts.Length - 1).ToArray();

            var parentName = pathParts.Length > 0 ? pathParts.Last() : "root";
            var parentPath = pathParts.Length > 0 ? pathParts.Aggregate((a, b) => $"{a}/{b}") : "";
            var parent = new NullDirectory(parentPath, parentName);

            return new File(parent, fileName) { LastWriteTime = dto.LastWriteTime, Length = dto.Length };
        }



        private abstract class SyncActionDto
        {
            [JsonProperty(Required = Required.Always)]
            public Guid Id { get; set; }

            // Parameterless constructor required for JsonSerializer
            protected SyncActionDto()
            {
                
            }

            protected SyncActionDto(SyncAction syncAction)
            {
                this.Id = syncAction.Id;
            }

        }

        private class AddFileSyncActionDto : SyncActionDto
        {
            public SyncParticipant Target { get; set; }

            public FileDto NewFile { get; set; }            

            // Parameterless constructor required for JsonSerializer
            public AddFileSyncActionDto()
            {
                
            }

            public AddFileSyncActionDto(AddFileSyncAction syncAction) : base(syncAction)
            {
                this.Target = syncAction.Target;
                this.NewFile = new FileDto(syncAction.NewFile);                
            }
        
        }

        private class RemoveFileSyncActionDto : SyncActionDto
        { 
            public SyncParticipant Target { get; set; }

            public FileDto RemovedFile { get; set; }

            // Parameterless constructor required for JsonSerializer
            public RemoveFileSyncActionDto()
            {
                
            }

            public RemoveFileSyncActionDto(RemoveFileSyncAction syncAction) : base(syncAction)
            {
                this.Target = syncAction.Target;
                this.RemovedFile = new FileDto(syncAction.RemovedFile);                
            }

        }


        private class ReplaceFileSyncActionDto : SyncActionDto
        {
            public SyncParticipant Target { get; set; }

            public FileDto OldVersion { get; set; }

            public FileDto NewVersion { get; set; }

            // Parameterless constructor required for JsonSerializer
            public ReplaceFileSyncActionDto()
            {
                
            }

            public ReplaceFileSyncActionDto(ReplaceFileSyncAction syncAction) : base(syncAction)
            {
                this.Target = syncAction.Target;
                this.OldVersion = new FileDto(syncAction.OldVersion);
                this.NewVersion = new FileDto(syncAction.NewVersion);
            }
        }

        private class FileDto
        {
            // Parameterless constructor required for JsonSerializer
            public FileDto()
            {
                
            }

            public FileDto(IFile file)
            {
                this.Path = file.Path;
                this.LastWriteTime = file.LastWriteTime;
                this.Length = file.Length;
            }
            
            public string Path { get; set; }

            public DateTime LastWriteTime { get; set; }

            public long Length { get; set; }
        }



    }
}