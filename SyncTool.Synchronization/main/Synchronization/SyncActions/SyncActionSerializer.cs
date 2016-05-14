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
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization.SyncActions
{
    public class SyncActionSerializer 
    {
        static readonly JsonSerializerSettings s_SerializerSettings = new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Include};


        public string Serialize(SyncAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var dto = new SyncActionDto(action);
            return JsonConvert.SerializeObject(dto, Formatting.Indented, s_SerializerSettings);
        }

        public void Serialize(SyncAction action, System.IO.Stream writeTo)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (writeTo == null)
            {
                throw new ArgumentNullException(nameof(writeTo));
            }

            var json = Serialize(action);
            var writer = new StreamWriter(writeTo);
            writer.Write(json);
            writer.Flush();
        }

        public SyncAction Deserialize(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                return Deserialize(json);
            }
        }

        public SyncAction Deserialize(string jsonString)
        {
            if (jsonString == null)
            {
                throw new ArgumentNullException(nameof(jsonString));
            }

            try
            {
                var syncActionDto = JsonConvert.DeserializeObject<SyncActionDto>(jsonString);

                return new SyncAction(
                    type: syncActionDto.Type,
                    fromVersion: DeserializeFileReference(syncActionDto.FromVersion),
                    toVersion: DeserializeFileReference(syncActionDto.ToVersion),
                    id: syncActionDto.Id,
                    target: syncActionDto.Target,
                    state: syncActionDto.State,
                    syncPointId: syncActionDto.SyncPointId);
            }
            catch (JsonSerializationException e)
            {
                throw new SerializationException("Error deserializing json as SyncAction", e);
            }
        }

        
        IFileReference DeserializeFileReference(FileReferenceDto dto)
        {   
            return dto != null ? new FileReference(dto.Path, dto.LastWriteTime, dto.Length) : null;
        }


        sealed class SyncActionDto
        {
            [JsonRequired]
            public string Target { get; set; }

            [JsonRequired]
            public Guid Id { get; set; }

            [JsonRequired, JsonConverter(typeof(StringEnumConverter))]
            public SyncActionState State { get; set; }

            [JsonRequired]
            public int SyncPointId { get; set; }

            [JsonRequired, JsonConverter(typeof(StringEnumConverter))]
            public ChangeType Type { get; set; }
            
            public FileReferenceDto FromVersion { get; set; }

            public FileReferenceDto ToVersion { get; set; }


            public SyncActionDto()
            {
                
            }

            public SyncActionDto(SyncAction action)
            {
                Target = action.Target;
                Id = action.Id;
                State = action.State;
                SyncPointId = action.SyncPointId;
                Type = action.Type;
                FromVersion = action.FromVersion != null ? new FileReferenceDto(action.FromVersion) : null;
                ToVersion = action.ToVersion != null ? new FileReferenceDto(action.ToVersion) : null;
            }
        }

        class FileReferenceDto 
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