using SyncTool.FileSystem.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    class SqlFileSystemSnapshot : IFileSystemSnapshot
    {
        readonly FileSystemSnapshotDo m_SnapshotDo;
        readonly IDatabaseContextFactory m_ContextFactory;
        readonly Lazy<IDirectory> m_RootDirectory;


        public IFileSystemHistory History { get; }

        public string Id => m_SnapshotDo.Id.ToString();

        public DateTime CreationTime => m_SnapshotDo.CreationTimeUtc;

        public IDirectory RootDirectory => m_RootDirectory.Value;


        public SqlFileSystemSnapshot(IDatabaseContextFactory contextFactory, SqlFileSystemHistory history, FileSystemSnapshotDo snapshotDo)
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            History = history ?? throw new ArgumentNullException(nameof(history));
            m_SnapshotDo = snapshotDo ?? throw new ArgumentNullException(nameof(snapshotDo));
            m_RootDirectory = new Lazy<IDirectory>(LoadRootDirectory);
        }


        IDirectory LoadRootDirectory()
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var dir = context.FileSystemSnapshots
                    .Where(x => x.Id == m_SnapshotDo.Id)
                    .Select(x => x.RootDirectory)
                    .Single();
                
                return new SqlDirectory(m_ContextFactory, null, dir);
            }
        }
    }
}
