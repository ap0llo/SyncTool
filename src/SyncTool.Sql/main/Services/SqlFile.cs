using Microsoft.EntityFrameworkCore;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using System;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlFile : FileSystemItem, IFile
    {
        readonly IDatabaseContextFactory m_ContextFactory;
        readonly FileInstanceDo m_FileDo;

        //TODO: Special-casing DateTime.MinValue seems wrong, needs to be investigated
        public DateTime LastWriteTime => m_FileDo.LastWriteTimeTicks == 0 ? DateTime.MinValue : new DateTime(m_FileDo.LastWriteTimeTicks, DateTimeKind.Utc);

        public long Length => m_FileDo.Length;

        public override string Name => m_FileDo.File.Name;


        public SqlFile(IDatabaseContextFactory contextFactory, IDirectory parent, FileInstanceDo fileDo) : base(parent, "Placeholder")
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            m_FileDo = Load(fileDo ?? throw new ArgumentNullException(nameof(fileDo)));
        }


        public IFile WithParent(IDirectory newParent) => throw new NotSupportedException();

        FileInstanceDo Load(FileInstanceDo instanceDo)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                return context.FileInstances
                    .Where(x => x.Id == instanceDo.Id)
                    .Include(x => x.File)
                    .Single();
            }
        }

    }
}
