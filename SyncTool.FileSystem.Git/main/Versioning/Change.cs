using System;

namespace SyncTool.FileSystem.Git
{
    public class Change : IChange
    {
        public ChangeType Type { get; }

        public IFile FromFile { get; }

        public IFile ToFile { get; }

        public Change(ChangeType type, IFile fromFile, IFile toFile)
        {          
            Type = type;
            FromFile = fromFile;
            ToFile = toFile;
        }
    }
}