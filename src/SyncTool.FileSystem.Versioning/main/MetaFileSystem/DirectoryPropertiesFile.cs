using System;

namespace SyncTool.FileSystem.Versioning.MetaFileSystem
{
    /// <summary>
    /// Represents a file containing a serialized <see cref="FileProperties"/> instance
    /// </summary>
    public class DirectoryPropertiesFile : DataFile<DirectoryProperties>
    {
        /// <summary>
        /// The name of the file
        /// </summary>
        public const string FileName = "SyncToolDirectoryInfo.json";


        private DirectoryPropertiesFile(IDirectory parentDirectory, IDirectory directory) : base(parentDirectory, FileName, new DirectoryProperties(directory))
        {                    
        }

        // constructor needs to be internal because it is used by tests
        internal DirectoryPropertiesFile(IDirectory parentDirectory, DateTime lastWriteTime, DirectoryProperties content) : base(parentDirectory, FileName, content)
        {            
            LastWriteTime = lastWriteTime;
        }
        
        
        public override IFile WithParent(IDirectory newParent)
        {
            return new DirectoryPropertiesFile(newParent, this.LastWriteTime, this.Content);
        }
      

        /// <summary>
        /// Creates a new <see cref="DirectoryPropertiesFile"/> encapsulating the properties of the specified directory instance
        /// </summary>
        public static DirectoryPropertiesFile ForDirectory(IDirectory parentDirectory, IDirectory directory) => new DirectoryPropertiesFile(parentDirectory, directory);

        /// <summary>
        /// Loads a <see cref="DirectoryPropertiesFile"/> written out into a file 
        /// </summary>
        public static DirectoryPropertiesFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.Equals(FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name has to be {FileName}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new DirectoryPropertiesFile(parentDirectory, file.LastWriteTime, stream.Deserialize<DirectoryProperties>());
            }
        }

    }
}