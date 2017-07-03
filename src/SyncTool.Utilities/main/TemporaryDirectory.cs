using System;
using System.IO;

namespace SyncTool.Utilities
{
    public sealed class TemporaryDirectory : IDisposable
    {

        public string FullName { get; }


        public TemporaryDirectory() : this(Path.GetTempPath())
        {            
        }

        public TemporaryDirectory(string basePath)
        {
            FullName = Path.Combine(basePath, Path.GetRandomFileName());
            Directory.CreateDirectory(FullName);
        }


        public void Dispose() => DirectoryHelper.DeleteRecursively(FullName);


        public static implicit operator string(TemporaryDirectory instance) => instance?.FullName;
    }
}
