using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Sql.Model.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.Sql.Services
{
    class SqlDirectory : AbstractDirectory
    {        
        readonly FileSystemRepository m_Repository;
        readonly DirectoryInstancesTable.Record m_DirectoryInstanceDo;
        readonly Lazy<IDictionary<string, SqlDirectory>> m_Directories;
        readonly Lazy<IDictionary<string, SqlFile>> m_Files;


        public override IEnumerable<IDirectory> Directories => m_Directories.Value.Values;

        public override IEnumerable<IFile> Files => m_Files.Value.Values;

        public override string Name => m_DirectoryInstanceDo.Directory.Name;


        public SqlDirectory(FileSystemRepository repository, IDirectory parent, DirectoryInstancesTable.Record directoryDo) 
            : base(parent, "Placeholder")
        {
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_DirectoryInstanceDo = directoryDo ?? throw new ArgumentNullException(nameof(directoryDo));            

            m_Directories = new Lazy<IDictionary<string, SqlDirectory>>(LoadDirectories);
            m_Files = new Lazy<IDictionary<string, SqlFile>>(LoadFiles);

            m_Repository.LoadDirectory(m_DirectoryInstanceDo);
        }
   

        protected override bool FileExistsByName(string name) => m_Files.Value.ContainsKey(name);

        protected override bool DirectoryExistsByName(string name) => m_Directories.Value.ContainsKey(name);

        protected override IFile GetFileByName(string name) => m_Files.Value[name];

        protected override IDirectory GetDirectoryByName(string name) => m_Directories.Value[name];


        IDictionary<string, SqlDirectory> LoadDirectories()
        {
            m_Repository.LoadDirectories(m_DirectoryInstanceDo);

            //TODO: Encapsulate creation of SqlDirectory instances
            return m_DirectoryInstanceDo.Directories
                        .Select(directoryDo => new SqlDirectory(m_Repository, this, directoryDo))
                        .ToDictionary(d => d.Name, StringComparer.InvariantCultureIgnoreCase);
        }

        IDictionary<string, SqlFile> LoadFiles()
        {
            m_Repository.LoadFiles(m_DirectoryInstanceDo);

            //TODO: Encapsulate creation of SqlFile instances
            return m_DirectoryInstanceDo.Files
                    .Select(fileDo => new SqlFile(m_Repository, this, fileDo))  
                    .ToDictionary(f => f.Name, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
