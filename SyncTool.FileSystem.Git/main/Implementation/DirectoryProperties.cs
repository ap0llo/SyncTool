using System;
using System.IO;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Serializable properties of a directory
    /// </summary>
    public class DirectoryProperties
    {

        /// <summary>
        /// The name of the directory
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DirectoryProperties"/> by copying all properties from the specified <see cref="IDirectory"/> instance
        /// </summary>
        public DirectoryProperties(IDirectory directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            this.Name = directory.Name;
        }

        /// <summary>
        /// Initializes a new (empty) instance of <see cref="DirectoryProperties"/>
        /// </summary>
        public DirectoryProperties()
        {
            
        }


        public void WriteTo(Stream stream)
        {
            var serializer = new JsonSerializer();
            var streamWriter = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };


            serializer.Serialize(jsonWriter, this);

            jsonWriter.Flush();
            streamWriter.Flush();
        }

      
        public static DirectoryProperties Load(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();

            return serializer.Deserialize<DirectoryProperties>(jsonReader);
        }

    }
}