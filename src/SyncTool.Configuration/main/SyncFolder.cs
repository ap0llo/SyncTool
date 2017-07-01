using System;

namespace SyncTool.Configuration
{
    //TODO: make class immutable
    /// <summary>
    /// Configuration object for a folder that's being synced within a group
    /// </summary>
    public sealed class SyncFolder : IEquatable<SyncFolder>
    {        
        /// <summary>
        /// Gets the name of the folder
        /// </summary>
        public string Name { get; }
      
        /// <summary>
        /// Gets or sets the path of the folders root directory in the filesystem
        /// </summary>
        public string Path { get; set; }


        public SyncFolder(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value must not be empty or whitespace", nameof(name));
            }

            Name = name;
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(Name);

        public override bool Equals(object obj) => Equals(obj as SyncFolder);

        public bool Equals(SyncFolder other)
            => other != null &&
               StringComparer.InvariantCultureIgnoreCase.Equals(Name, other.Name) &&
               StringComparer.InvariantCultureIgnoreCase.Equals(Path, other.Path);
    }
}