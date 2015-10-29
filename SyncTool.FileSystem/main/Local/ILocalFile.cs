namespace SyncTool.FileSystem.Local
{
    public interface ILocalFile : IFile
    {
        /// <summary>
        /// The full path of the file in the local file system
        /// </summary>
         string Location { get; }
    }
}