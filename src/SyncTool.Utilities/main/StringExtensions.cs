using System;
using System.IO;

namespace SyncTool.Utilities
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


        public static bool StartsWith(this string str, char c)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Length > 0 && str[0] == c;
        }

        public static bool EndsWith(this string str, char c)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            return str.Length > 0 && str[str.Length - 1] == c;
        }
    }
}