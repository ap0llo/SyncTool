using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Sql.Model.Tables;
using System;

namespace SyncTool.Sql.Services
{
    class SqlFile : FileSystemItem, IFile
    {
        readonly FileSystemRepository m_Repository;
        readonly Database m_Database;
        readonly FileInstancesTable.Record m_FileInstance;

        //TODO: Special-casing DateTime.MinValue seems wrong, needs to be investigated
        public DateTime LastWriteTime => m_FileInstance.LastWriteTimeTicks == 0 ? DateTime.MinValue : new DateTime(m_FileInstance.LastWriteTimeTicks, DateTimeKind.Utc);

        public long Length => m_FileInstance.Length;

        public override string Name => m_FileInstance.File.Name;


        public SqlFile(FileSystemRepository repository, IDirectory parent, FileInstancesTable.Record fileInstance) : base(parent, "Placeholder")
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_FileInstance = fileInstance ?? throw new ArgumentNullException(nameof(fileInstance));

            m_Repository.LoadFile(m_FileInstance);
        }


        public IFile WithParent(IDirectory newParent) => throw new NotSupportedException();        
    }
}
