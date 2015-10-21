namespace SyncTool.FileSystem
{
    public abstract class FileSystemItem
    {
        public string Name { get; set; }

        public string Path => Parent != null ? Parent.Path + "/" + Name : Name;

        public Directory Parent { get; set; }

    }
}