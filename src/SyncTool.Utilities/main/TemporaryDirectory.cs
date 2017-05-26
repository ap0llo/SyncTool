using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Utilities
{
    public sealed class TemporaryDirectory : IDisposable
    {
        public string FullName { get; }


        public TemporaryDirectory()
        {
            FullName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(FullName);
        }


        public void Dispose() => DirectoryHelper.DeleteRecursively(FullName);


        public static implicit operator string(TemporaryDirectory instance) => instance?.FullName;
    }
}
