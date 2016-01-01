using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SyncTool.Common
{
    public static class StringExtensions
    {        
        /// <summary>
        /// Creates a in-memory stream and writes the string content to it
        /// </summary>
        public static Stream ToStream(this string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

    }
}