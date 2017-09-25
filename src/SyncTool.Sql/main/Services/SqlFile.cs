using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using System;
using NodaTime;

namespace SyncTool.Sql.Services
{
    class SqlFile : FileSystemItem, IFile
    {           
        readonly FileInstanceDo m_FileInstance;

        public Instant LastWriteTime => Instant.FromUnixTimeTicks(m_FileInstance.LastWriteUnixTimeTicks);
        
        public long Length => m_FileInstance.Length;

        public override string Name => m_FileInstance.File.Name;


        public SqlFile(FileSystemRepository repository, IDirectory parent, FileInstanceDo fileInstance) : base(parent, "Placeholder")
        {
            repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_FileInstance = fileInstance ?? throw new ArgumentNullException(nameof(fileInstance));

            repository.LoadFile(m_FileInstance);
        }


        public IFile WithParent(IDirectory newParent) => throw new NotSupportedException();        
    }
}
