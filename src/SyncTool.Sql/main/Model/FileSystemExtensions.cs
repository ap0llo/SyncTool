using SyncTool.FileSystem;
using SyncTool.Sql.Model.Tables;

namespace SyncTool.Sql.Model
{

    /// <summary>
    /// Extension methods for types from <see cref="SyncTool.FileSystem"/>
    /// </summary>
    static class FileSystemExtensions
    {
        public static DirectoryInstancesTable.Record ToDirectoryInstanceDo(this IDirectory directory)
        {
            var instanceDo = new DirectoryInstancesTable.Record(DirectoriesTable.Record.FromDirectory(directory));

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

        public static FileInstancesTable.Record ToFileInstanceDo(this IFile file) => new FileInstancesTable.Record(FilesTable.Record.FromFile(file), file.LastWriteTime, file.Length);

    }
}
