using SyncTool.FileSystem.Versioning;
using System;
using JetBrains.Annotations;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using NodaTime;

namespace SyncTool.Sql.Services
{
    class SqlFileSystemSnapshot : IFileSystemSnapshot
    {
        readonly ISqlFileSystemFactory m_FileSystemFactory;
        readonly FileSystemRepository m_FileSystemRepository;
        readonly FileSystemSnapshotDo m_SnapshotDo;
        readonly Lazy<IDirectory> m_RootDirectory;


        public IFileSystemHistory History { get; }

        public string Id => m_SnapshotDo.Id.ToString();

        public Instant CreationTime => Instant.FromUnixTimeTicks(m_SnapshotDo.CreationUnixTimeTicks);

        public IDirectory RootDirectory => m_RootDirectory.Value;


        public SqlFileSystemSnapshot(
            [NotNull] FileSystemRepository fileSystemRepository, 
            [NotNull] ISqlFileSystemFactory fileSystemFactory, 
            [NotNull] SqlFileSystemHistory history, 
            [NotNull] FileSystemSnapshotDo snapshotDo)
        {
            m_FileSystemFactory = fileSystemFactory ?? throw new ArgumentNullException(nameof(fileSystemFactory));
            m_FileSystemRepository = fileSystemRepository ?? throw new ArgumentNullException(nameof(fileSystemRepository));
            History = history ?? throw new ArgumentNullException(nameof(history));
            m_SnapshotDo = snapshotDo ?? throw new ArgumentNullException(nameof(snapshotDo));
            m_RootDirectory = new Lazy<IDirectory>(LoadRootDirectory);
        }


        IDirectory LoadRootDirectory()
        {            
            var directoryInstance = m_FileSystemRepository.GetDirectoryInstance(m_SnapshotDo.RootDirectoryInstanceId);    
            return m_FileSystemFactory.CreateSqlDirectory(null, directoryInstance);            
        }
    }
}
