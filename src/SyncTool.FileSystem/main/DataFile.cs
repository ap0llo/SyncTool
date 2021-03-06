﻿using System;
using System.IO;

namespace SyncTool.FileSystem
{
    /// <summary>
    /// Implementation of <see cref="IReadableFile"/> with arbitrary content of type <typeparam name="T" />
    /// When OpenRead() is callled, the content will be serialized to Json and written to the stream
    /// </summary>
    /// <typeparam name="T"/>
    public abstract class DataFile<T> : FileSystemItem, IReadableFile where T : class
    {
        public T Content { get; }

        public DateTime LastWriteTime { get; protected set; }

        public long Length => throw new NotSupportedException();


        protected DataFile(IDirectory parent, string name, T content) : base(parent, name)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            LastWriteTime = DateTime.Now;
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