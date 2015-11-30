// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Security.AccessControl;

namespace SyncTool.FileSystem.Git
{
    public class RepositoryInfoFile : FileSystemItem, IReadableFile
    {
        public const string RepositoryInfoFileName = "SyncToolRepositoryInfo.json";

        
        public DateTime LastWriteTime { get; }

        public long Length
        {
            get { throw new NotImplementedException(); }
        }

        public RepositoryInfo Content { get; }


        public RepositoryInfoFile(IDirectory parent, string repositoryName = null) : base(parent, RepositoryInfoFileName)
        {
            LastWriteTime = DateTime.Now;
            Content = repositoryName == null ? new RepositoryInfo() : new RepositoryInfo(repositoryName);
        }


        public Stream OpenRead()
        {
            using (var writeStream = new MemoryStream())
            {
                Content.WriteTo(writeStream);
                writeStream.Flush();

                return new MemoryStream(writeStream.ToArray());
            }
        }

        public IFile WithParent(IDirectory newParent)
        {
            return  new RepositoryInfoFile(newParent, this.Content.RepositoryName);
        }
    }
}