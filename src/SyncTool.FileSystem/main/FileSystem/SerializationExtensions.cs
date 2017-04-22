using System;
using System.IO;
using Newtonsoft.Json;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Extension methods for (de)serialization
    /// </summary>
    public static class SerializationExtensions
    {

        /// <summary>
        /// Serializes the object to JSON and writes it to the specified stream
        /// </summary>
        public static void WriteTo(this object obj, Stream stream)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var serializer = new JsonSerializer();
            var streamWriter = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.Indented };

            serializer.Serialize(jsonWriter, obj);

            jsonWriter.Flush();
            streamWriter.Flush();
        }
        

        /// <summary>
        /// Reads a object of type <typeparamref name="T"/> serialized to JSON from the stream 
        /// </summary>
        public static T Deserialize<T>(this Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();

            return serializer.Deserialize<T>(jsonReader);
        }


    }
}