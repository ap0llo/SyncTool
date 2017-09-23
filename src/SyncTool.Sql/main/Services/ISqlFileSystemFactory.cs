using SyncTool.FileSystem;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    /// <summary>
    /// Defines interface for factory that creates instances of <see cref="SqlFile" /> and <see cref="SqlDirectory"/>
    /// </summary>
    interface ISqlFileSystemFactory
    {
        SqlDirectory CreateSqlDirectory(IDirectory parent, DirectoryInstanceDo directoryDo);

        SqlFile CreateSqlFile(IDirectory parent, FileInstanceDo fileInstance);
    }
}