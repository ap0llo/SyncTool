// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.SyncActions
{
    public class SyncActionSerializer : ISyncActionVisitor<JObject>
    {
        const string s_Name = "name";
        const string s_Value = "value";
                


        public string Serialize(SyncAction action)
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

            return jObject.ToString();

        }


        public SyncAction Deserialize(string jsonString)
        {
            var json = JObject.Parse(jsonString);

            var name = GetPropertyValue(json, s_Name, JTokenType.String).ToString();
            var value = GetObjectProperty(json, s_Value);
           
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


        AddFileSyncAction DeserializeAddFileSyncAction(JObject json)
        {
            var dto = JsonConvert.DeserializeObject<AddFileSyncActionDto>(json.ToString());
            return new AddFileSyncAction(dto.Target, DeserializeFile(dto.NewFile));
        }

        RemoveFileSyncAction DeserializeRemoveFileSyncAction(JObject json)
        {
            var dto = JsonConvert.DeserializeObject<RemoveFileSyncActionDto>(json.ToString());
            return new RemoveFileSyncAction(dto.Target, DeserializeFile(dto.RemovedFile));
            
        }

        ReplaceFileSyncAction DeserializeReplaceFileSyncAction(JObject json)
        {
            var dto = JsonConvert.DeserializeObject<ReplaceFileSyncActionDto>(json.ToString());
            return new ReplaceFileSyncAction(dto.Target, DeserializeFile(dto.OldVersion), DeserializeFile(dto.NewVersion));            
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
            var property = jObject[propertyName] as JToken;
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
        

        private class AddFileSyncActionDto
        {
            public SyncParticipant Target { get; set; }

            public FileDto NewFile { get; set; }


            // Parameterless constructor required for JsonSerializer
            public AddFileSyncActionDto()
            {
                
            }

            public AddFileSyncActionDto(AddFileSyncAction action)
            {
                this.Target = action.Target;
                this.NewFile = new FileDto(action.NewFile);
            }
        
        }

        private class RemoveFileSyncActionDto
        {
            public SyncParticipant Target { get; set; }

            public FileDto RemovedFile { get; set; }

            // Parameterless constructor required for JsonSerializer
            public RemoveFileSyncActionDto()
            {
                
            }

            public RemoveFileSyncActionDto(RemoveFileSyncAction syncAction)
            {
                this.Target = syncAction.Target;
                this.RemovedFile = new FileDto(syncAction.RemovedFile);
            }

        }


        private class ReplaceFileSyncActionDto
        {
            public SyncParticipant Target { get; set; }

            public FileDto OldVersion { get; set; }

            public FileDto NewVersion { get; set; }

            // Parameterless constructor required for JsonSerializer
            public ReplaceFileSyncActionDto()
            {
                
            }

            public ReplaceFileSyncActionDto(ReplaceFileSyncAction syncAction)
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