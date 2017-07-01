using System;
using SyncTool.FileSystem;
using SyncTool.Synchronization.Conflicts;

namespace SyncTool.Git.Synchronization.Conflicts
{
    public class ConflictInfoFile : DataFile<ConflictInfo>
    {
        public const string FileNameSuffix = ".SyncConflict.json";
        
        public ConflictInfoFile(IDirectory parent, ConflictInfo content) : base(parent, PathParser.GetFileName(content.FilePath) + FileNameSuffix, content)
        {
        }

        private ConflictInfoFile(IDirectory parent, string name, ConflictInfo content) : base(parent, name, content)
        {
        }


        public override IFile WithParent(IDirectory newParent) => new ConflictInfoFile(newParent, Name, Content);


        /// <summary>
        /// Loads a <see cref="ConflictInfoFile"/> written out into a file 
        /// </summary>
        public static ConflictInfoFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            if (!file.Name.EndsWith(FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentException($"File name Name must end with {FileNameSuffix}", nameof(file));

            using (var stream = file.OpenRead())
            {
                return new ConflictInfoFile(parentDirectory, file.Name, stream.Deserialize<ConflictInfo>());
            }
        }
    }
}