// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public abstract class DataFile<T> : FileSystemItem, IReadableFile
    {
        public T Content { get; }

        public DateTime LastWriteTime { get; protected set; }

        public long Length { get { throw new NotSupportedException(); } }


        protected DataFile(IDirectory parent, string name, T content) : base(parent, name)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            this.Content = content;
            this.LastWriteTime = DateTime.Now;
        }


        public virtual Stream OpenRead()
        {
            using (var writeStream = new MemoryStream())
            {
                Content.WriteTo(writeStream);
                writeStream.Flush();

                return new MemoryStream(writeStream.ToArray());
            }
        }

        public abstract IFile WithParent(IDirectory newParent);
    }
}