using System;
using SyncTool.FileSystem;
using SyncTool.Git.Common;

namespace SyncTool.Git.FileSystem
{
    public class RepositoryInfoFile : DataFile<RepositoryInfo>
    {
        public const string RepositoryInfoFileName = "SyncToolRepositoryInfo.json";
        
        

        public RepositoryInfoFile(IDirectory parent) : this(parent, new RepositoryInfo())
        {
         
        }

        public RepositoryInfoFile(IDirectory parent, RepositoryInfo repositoryInfo) : base(parent, RepositoryInfoFileName, repositoryInfo)
        {            
        }

        public RepositoryInfoFile(IDirectory parent, RepositoryInfo repositoryInfo, DateTime lastWriteTime) : base(parent, RepositoryInfoFileName, repositoryInfo)
        {
            this.LastWriteTime = lastWriteTime;
        }




        public override IFile WithParent(IDirectory newParent)
        {
            return new RepositoryInfoFile(newParent, this.Content);
        }


        /// <summary>
        /// Loads a <see cref="RepositoryInfoFile"/> written out into a file 
        /// </summary>
        public static RepositoryInfoFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.Equals(RepositoryInfoFileName, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name has to be {RepositoryInfoFileName}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new RepositoryInfoFile(parentDirectory, stream.Deserialize<RepositoryInfo>(), file.LastWriteTime);
            }
        }

    }
}