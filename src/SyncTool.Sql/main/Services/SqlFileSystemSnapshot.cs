using SyncTool.FileSystem.Versioning;
using System;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Sql.Model.Tables;

namespace SyncTool.Sql.Services
{
    class SqlFileSystemSnapshot : IFileSystemSnapshot
    {
        readonly FileSystemRepository m_FileSystemRepository;
        readonly FileSystemSnapshotsTable.Record m_SnapshotDo;
        readonly Lazy<IDirectory> m_RootDirectory;


        public IFileSystemHistory History { get; }

        public string Id => m_SnapshotDo.Id.ToString();

        public DateTime CreationTime => new DateTime(m_SnapshotDo.CreationTimeTicks, DateTimeKind.Utc);

        public IDirectory RootDirectory => m_RootDirectory.Value;


        public SqlFileSystemSnapshot(FileSystemRepository fileSystemRepository, SqlFileSystemHistory history, FileSystemSnapshotsTable.Record snapshotDo)
        {            
            m_FileSystemRepository = fileSystemRepository ?? throw new ArgumentNullException(nameof(fileSystemRepository));
            History = history ?? throw new ArgumentNullException(nameof(history));
            m_SnapshotDo = snapshotDo ?? throw new ArgumentNullException(nameof(snapshotDo));
            m_RootDirectory = new Lazy<IDirectory>(LoadRootDirectory);
        }


        IDirectory LoadRootDirectory()
        {
            //TODO: Encapsulate creation of SqlDirectory instances
            var directoryInstance = m_FileSystemRepository.GetDirectoryInstance(m_SnapshotDo.RootDirectoryInstanceId);    
            return new SqlDirectory(m_FileSystemRepository, null, directoryInstance);            
        }
    }
}
