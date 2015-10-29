using System;
using System.IO;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Class that provides a simple way to store properties of a <see cref="IFile"/>.    
    /// </summary>
    public class FileProperties : IEquatable<FileProperties>, IFile
    {

        /// <summary>
        /// The Name of the file
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The time the file was last modified
        /// </summary>
        public DateTime LastWriteTime { get; set; }

        /// <summary>
        /// The size of the file in bytes
        /// </summary>
        public long Length { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="FileProperties"/> by copying all properties from the specified <see cref="IFile"/> instance
        /// </summary>
        public FileProperties(IFile file)
        {
            LastWriteTime = file.LastWriteTime;
            Length = file.Length;
            Name = file.Name;
        }

        /// <summary>
        /// Initializes a new (empty) instance of <see cref="FileProperties"/>
        /// </summary>
        public FileProperties()
        {
        }



        /// <summary>
        /// Serializes this <see cref="FileProperties"/> instance and writes it to the specified stream
        /// </summary>
        public void WriteTo(Stream stream)
        {
            var serializer = new JsonSerializer();
            var streamWriter = new StreamWriter(stream);
            var jsonWriter = new JsonTextWriter(streamWriter) {Formatting = Formatting.Indented};


            serializer.Serialize(jsonWriter, this);

            jsonWriter.Flush();
            streamWriter.Flush();
        }



        /// <summary>
        /// Reads a <see cref="FileProperties"/> instance from the specified stream
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static FileProperties Load(Stream stream)
        {
            var streamReader = new StreamReader(stream);
            var jsonReader = new JsonTextReader(streamReader);
            var serializer = new JsonSerializer();

            return serializer.Deserialize<FileProperties>(jsonReader);
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(this.Name);

        public override bool Equals(object obj) => Equals(obj as FileProperties);

        public bool Equals(FileProperties other)
        {
            if (other == null)
            {
                return false;
            }

            return StringComparer.InvariantCultureIgnoreCase.Equals(this.Name, other.Name) &&
                   this.LastWriteTime == other.LastWriteTime &&
                   this.Length == other.Length;
        }
    }
}