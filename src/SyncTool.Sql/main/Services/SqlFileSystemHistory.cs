using SyncTool.FileSystem.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using System.Diagnostics;

namespace SyncTool.Sql.Services
{    
    class SqlFileSystemHistory : IFileSystemHistory
    {
        readonly IDatabaseContextFactory m_ContextFactory;
        readonly Func<SqlFileSystemHistory, FileSystemSnapshotDo, SqlFileSystemSnapshot> m_SnapshotFactory;
        readonly FileSystemHistoryDo m_HistoryDo;


        public IFileSystemSnapshot this[string id]
        {
            get
            {
                var dbId = ParseId(id);
                var snapshotDo = GetSnapshotDo(dbId);
                
                return m_SnapshotFactory.Invoke(this, snapshotDo);
            }
        }

       

        public string Name => m_HistoryDo.Name;

        public string Id => m_HistoryDo.Id.ToString();

        public IFileSystemSnapshot LatestFileSystemSnapshot
        {
            get
            {
                using (var context = m_ContextFactory.CreateContext())
                {
                    var snapshotDo = context
                        .FileSystemSnapshots
                        .Where(x => x.History.Id == m_HistoryDo.Id)
                        .OrderByDescending(x => x.SequenceNumber)
                        .FirstOrDefault();

                    return snapshotDo == null ? null : m_SnapshotFactory.Invoke(this, snapshotDo);                    
                }
            }
        }

        public IEnumerable<IFileSystemSnapshot> Snapshots
        {
            get
            {                
                using (var context = m_ContextFactory.CreateContext())
                {
                    return context
                        .FileSystemSnapshots
                        .Where(snapshot => snapshot.History.Id == m_HistoryDo.Id)
                        .OrderByDescending(snapshot => snapshot.SequenceNumber)
                        .ToArray()
                        .Select(snapshotDo => m_SnapshotFactory.Invoke(this, snapshotDo));
                }
            }
        }


        
        public SqlFileSystemHistory(IDatabaseContextFactory contextFactory, Func<SqlFileSystemHistory, FileSystemSnapshotDo,SqlFileSystemSnapshot> snapshotFactory, FileSystemHistoryDo historyDo)
        {
            m_ContextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            m_SnapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
            m_HistoryDo = historyDo ?? throw new ArgumentNullException(nameof(historyDo));
        }

        public IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var fileInstances = new List<FileInstanceDo>();
                var directoryInstanceDo = AddDirectoryInstance(context, fileInstances, fileSystemState);

                var sequenceNumber = context.FileSystemSnapshots.Count() + 1;
                var snapshotDo = new FileSystemSnapshotDo(m_HistoryDo, DateTime.UtcNow, directoryInstanceDo, fileInstances, sequenceNumber);
                context.FileSystemSnapshots.Add(snapshotDo);

                // update the version proeprty so confllcits can be detected                
                m_HistoryDo.Version += 1;
                context.Update(m_HistoryDo);

                context.SaveChanges();

                Debug.Assert(snapshotDo.Id != 0);

                return m_SnapshotFactory.Invoke(this, snapshotDo);
            }
        }

        public string[] GetChangedFiles(string toId)
        {
            throw new NotImplementedException();
        }

        public string[] GetChangedFiles(string fromId, string toId)
        {
            throw new NotImplementedException();
        }

        public IFileSystemDiff GetChanges(string toId, string[] pathFilter = null)
        {
            throw new NotImplementedException();
        }

        public IFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            throw new NotImplementedException();
        }

        public string GetPreviousSnapshotId(string id)
        {            
            var snapshotDo = GetSnapshotDo(ParseId(id));
            using (var context = m_ContextFactory.CreateContext())
            {
                var previousSnapshot = context
                    .FileSystemSnapshots
                    .Where(x => x.History.Id == m_HistoryDo.Id)
                    .Where(x => x.SequenceNumber < snapshotDo.SequenceNumber)
                    .OrderByDescending(x => x.SequenceNumber)
                    .FirstOrDefault();

                return (previousSnapshot?.Id)?.ToString();
            }
        }

      

        private DirectoryInstanceDo AddDirectoryInstance(DatabaseContext context, List<FileInstanceDo> allFileInstances, IDirectory directory)
        {
            var directoryDo = GetOrAddDirectory(context, directory);

            var instanceDo = new DirectoryInstanceDo(directoryDo);
            context.DirectoryInstances.Add(instanceDo);

            foreach(var dir in directory.Directories)
            {
                instanceDo.Directories.Add(AddDirectoryInstance(context, allFileInstances, dir));
            }

            foreach(var file in directory.Files)
            {
                instanceDo.Files.Add(GetOrAddFileInstance(context, allFileInstances, file));
            }

            return instanceDo;
        }

        private DirectoryDo GetOrAddDirectory(DatabaseContext context, IDirectory dir)
        {
            var normalizedPath = GetNormalizedPath(dir);

            var directoryDo = context.Directories.SingleOrDefault(d => d.NormalizedPath == normalizedPath);

            if(directoryDo == null)
            {
                directoryDo = DirectoryDo.FromDirectory(dir);                
                context.Directories.Add(directoryDo);
            }

            return directoryDo;
        }

        private FileDo GetOrAddFile(DatabaseContext context, IFile file)
        {            
            var fileDo = context.Files.SingleOrDefault(d => d.NormalizedPath == file.Path.NormalizeCaseInvariant());

            if(fileDo == null)
            {
                fileDo = FileDo.FromFile(file);
                context.Files.Add(fileDo);
            }

            return fileDo;
        }

        private FileInstanceDo GetOrAddFileInstance(DatabaseContext context, List<FileInstanceDo> allFileInstances, IFile file)
        {
            var instanceDo = default(FileInstanceDo);

            var fileDo = GetOrAddFile(context, file);

            instanceDo = context
                .FileInstances
                .Where(instance => instance.File.Id == fileDo.Id)
                .Where(instance => instance.Length == file.Length && instance.LastWriteTimeTicks == file.LastWriteTime.Ticks)
                .SingleOrDefault();                               
                
            if (instanceDo == null)
            {
                instanceDo = new FileInstanceDo(fileDo, file.LastWriteTime, file.Length);
                context.FileInstances.Add(instanceDo);
            }

            allFileInstances.Add(instanceDo);
            return instanceDo;
        }

        private string GetNormalizedPath(IFileSystemItem item)
        {            
            return item.Path.ToLowerInvariant();
        }

        private static int ParseId(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException("Value cannot be null or whitespace");
            }

            return int.Parse(id);            
        }

        private FileSystemSnapshotDo GetSnapshotDo(int dbId)
        {
            using (var context = m_ContextFactory.CreateContext())
            {
                var result = context
                        .FileSystemSnapshots
                        .SingleOrDefault(snapshot => snapshot.Id == dbId);

                return result ?? throw new SnapshotNotFoundException(dbId.ToString());                
            }
        }
    }
}
