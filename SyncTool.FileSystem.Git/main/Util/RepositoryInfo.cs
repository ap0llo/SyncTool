using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfo
    {

        public Version SyncToolVersion { get; set; }


        public RepositoryInfo()
        {
            SyncToolVersion = Assembly.GetExecutingAssembly().GetName().Version;
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
    }
}