using SyncTool.FileSystem;

namespace SyncTool.Sql.Model
{

    /// <summary>
    /// Extension methods for types from <see cref="SyncTool.FileSystem"/>
    /// </summary>
    static class FileSystemExtensions
    {
        public static DirectoryInstanceDo ToDirectoryInstanceDo(this IDirectory directory)
        {
            var instanceDo = new DirectoryInstanceDo(DirectoryDo.FromDirectory(directory));

            foreach(var child in directory.Directories)
            {
                instanceDo.Directories.Add(child.ToDirectoryInstanceDo());
            }

            foreach(var child in directory.Files)
            {
                instanceDo.Files.Add(child.ToFileInstanceDo());
            }

            return instanceDo;
        }

        public static FileInstanceDo ToFileInstanceDo(this IFile file) => new FileInstanceDo(FileDo.FromFile(file), file.LastWriteTime, file.Length);

    }
}
