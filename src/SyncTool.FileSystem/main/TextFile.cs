using System;
using System.IO;

namespace SyncTool.FileSystem
{
    public class TextFile : FileSystemItem, IReadableFile
    {
        public string Content { get; }

        public DateTime LastWriteTime { get; protected set; }

        public long Length => throw new NotSupportedException();


        public TextFile(IDirectory parent, string name, string content) : base(parent, name)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            LastWriteTime = DateTime.Now;
        }


        public virtual Stream OpenRead()
        {
            using (var writeStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(writeStream))
            {
                streamWriter.Write(Content);
                streamWriter.Flush();

                return new MemoryStream(writeStream.ToArray());
            }
        }

        public IFile WithParent(IDirectory newParent) => new TextFile(newParent, Name, Content);

        
        public static TextFile Load(IDirectory parentDirectory, IReadableFile file)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            using (var reader = new StreamReader(file.OpenRead()))
            {
                var content = reader.ReadToEnd();
                return new TextFile(parentDirectory, file.Name, content);
            }
        }
    }
}