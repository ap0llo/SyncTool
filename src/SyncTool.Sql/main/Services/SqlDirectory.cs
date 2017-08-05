using Microsoft.EntityFrameworkCore;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlDirectory : AbstractDirectory
    {
        readonly IDatabaseContextFactory m_ContextFactory;
        readonly DirectoryInstanceDo m_DirectoryInstanceDo;
        readonly Lazy<IDictionary<string, SqlDirectory>> m_Directories;
        readonly Lazy<IDictionary<string, SqlFile>> m_Files;


        public override IEnumerable<IDirectory> Directories => m_Directories.Value.Values;

        public override IEnumerable<IFile> Files => m_Files.Value.Values;

        public override string Name => m_DirectoryInstanceDo.Directory.Name;


        public SqlDirectory(IDatabaseContextFactory contextFactory, IDirectory parent, DirectoryInstanceDo directoryDo) 
            : base(parent, "Placeholder")
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            m_DirectoryInstanceDo = Load(directoryDo ?? throw new ArgumentNullException(nameof(directoryDo)));

            m_Directories = new Lazy<IDictionary<string, SqlDirectory>>(LoadDirectories);
            m_Files = new Lazy<IDictionary<string, SqlFile>>(LoadFiles);
        }
   

        protected override bool FileExistsByName(string name) => m_Files.Value.ContainsKey(name);

        protected override bool DirectoryExistsByName(string name) => m_Directories.Value.ContainsKey(name);

        protected override IFile GetFileByName(string name) => m_Files.Value[name];

        protected override IDirectory GetDirectoryByName(string name) => m_Directories.Value[name];


        IDictionary<string, SqlDirectory> LoadDirectories()
        {
            // load directories from database
            using (var context = m_ContextFactory.CreateContext())
            {
                var instanceDo = context
                    .DirectoryInstances
                    .Where(x => x.Id == m_DirectoryInstanceDo.Id)                    
                    .Include(x => x.Directories)
                    .Single();
             
                return instanceDo.Directories
                            .Select(directoryDo => new SqlDirectory(m_ContextFactory, this, directoryDo))
                            .ToDictionary(d => d.Name, StringComparer.InvariantCultureIgnoreCase);
            }
        }


        DirectoryInstanceDo Load(DirectoryInstanceDo instanceDo)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                return context.DirectoryInstances
                    .Where(x => x.Id == instanceDo.Id)
                    .Include(x => x.Directory)
                    .Single();                
            }                          
        }

        IDictionary<string, SqlFile> LoadFiles()
        {
            // load files from database
            using (var context = m_ContextFactory.CreateContext())
            {
                var instanceDo = context
                    .DirectoryInstances
                    .Where(x => x.Id == m_DirectoryInstanceDo.Id)
                    .Include(x => x.Files)
                    .Single();

                return instanceDo.Files
                        .Select(fileDo => new SqlFile(m_ContextFactory, this, fileDo))
                        .ToDictionary(f => f.Name, StringComparer.InvariantCultureIgnoreCase);
            }

        }
    }
}
