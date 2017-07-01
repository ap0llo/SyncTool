using System;
using System.IO;

namespace SyncTool.Common.Groups
{
    public sealed class GroupStorage
    {
        public string Path { get; }


        public GroupStorage(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be empty", nameof(path));

            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory '{path}' does not exist");

            Path = System.IO.Path.GetFullPath(path);
        }
    }
}
