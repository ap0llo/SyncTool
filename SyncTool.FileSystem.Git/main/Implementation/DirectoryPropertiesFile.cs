using System;
using System.IO;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    /// Represents a file containing a serialized <see cref="FileProperties"/> instance
    /// </summary>
    public class DirectoryPropertiesFile : IReadableFile
    {

        /// <summary>
        /// The name of the file
        /// </summary>
        public const string FileName = "SyncToolDirectoryInfo.json";


        private DirectoryPropertiesFile(IDirectory directory)
        {            
            Content = new DirectoryProperties(directory);
            LastWriteTime = DateTime.Now;
        }

        // constructor needs to be internal because it is used by tests
        internal DirectoryPropertiesFile(DateTime lastWriteTime, DirectoryProperties content)
        {            
            Content = content;
            LastWriteTime = lastWriteTime;
        }


        public string Name => FileName;

        public DateTime LastWriteTime { get; }

        public long Length { get { throw new NotSupportedException(); } }

        /// <summary>
        /// Gets the <see cref="DirectoryProperties"/> this file contains
        /// </summary>
        public DirectoryProperties Content { get; }

        public Stream Open(FileMode mode)
        {
            if (mode != FileMode.Open)
            {
                throw new NotSupportedException($"{nameof(FilePropertiesFile)} Open() only supports reading");
            }

            using (var writeStream = new MemoryStream())
            {
                Content.WriteTo(writeStream);
                writeStream.Flush();

                return new MemoryStream(writeStream.ToArray());
            }
        }


        /// <summary>
        /// Creates a new <see cref="DirectoryPropertiesFile"/> encapsulating the properties of the specified directory instance
        /// </summary>
        public static DirectoryPropertiesFile ForDirectory(IDirectory directory) => new DirectoryPropertiesFile(directory);

        /// <summary>
        /// Loads a <see cref="DirectoryPropertiesFile"/> written out into a file 
        /// </summary>
        public static DirectoryPropertiesFile Load(IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.Equals(FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name has to be {FileName}", nameof(file));
            }

            using (var stream = file.Open(FileMode.Open))
            {
                return new DirectoryPropertiesFile(file.LastWriteTime, DirectoryProperties.Load(stream));
            }
        }

    }
}