using SyncTool.FileSystem.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{    
    class SqlFileSystemHistory : IFileSystemHistory
    {
        readonly SnapshotRepository m_SnapshotRepository;
        readonly FileSystemRepository m_FileSystemRepository;        
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
                var snapshotDo = m_SnapshotRepository.GetLatestSnapshotOrDefault(m_HistoryDo.Id);
                return snapshotDo == null ? null : m_SnapshotFactory.Invoke(this, snapshotDo);                    
            }
        }

        public IEnumerable<IFileSystemSnapshot> Snapshots => 
            m_SnapshotRepository
                .GetSnapshots(m_HistoryDo.Id)
                .Select(snapshotDo => m_SnapshotFactory.Invoke(this, snapshotDo));


        public SqlFileSystemHistory(            
            SnapshotRepository snapshotRepository,
            FileSystemRepository fileSystemRepository,           
            Func<SqlFileSystemHistory, FileSystemSnapshotDo,SqlFileSystemSnapshot> snapshotFactory, 
            FileSystemHistoryDo historyDo)
        {            
            m_SnapshotRepository = snapshotRepository ?? throw new ArgumentNullException(nameof(snapshotRepository));
            m_FileSystemRepository = fileSystemRepository ?? throw new ArgumentNullException(nameof(fileSystemRepository));            

            m_SnapshotFactory = snapshotFactory ?? throw new ArgumentNullException(nameof(snapshotFactory));
            m_HistoryDo = historyDo ?? throw new ArgumentNullException(nameof(historyDo));
        }


        public IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState)
        {            
            var directoryInstance = fileSystemState.ToDirectoryInstanceDo();            
            m_FileSystemRepository.AddRecursively(directoryInstance);

            var snapshotDo = new FileSystemSnapshotDo(m_HistoryDo.Id, DateTime.UtcNow.Ticks, directoryInstance.Id, directoryInstance.GetFilesRecursively());            
            m_SnapshotRepository.AddSnapshot(snapshotDo);

            return m_SnapshotFactory.Invoke(this, snapshotDo);           
        }

        public string[] GetChangedFiles(string toId)
        {
            var changedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            
            // load snapshot to ensure it exists
            var toSnapshot = GetSnapshotDo(ParseId(toId));

            // iterate over all snpshot and get the changed files for every item
            var currentSnapshot = toSnapshot;
            while (currentSnapshot != null)
            {
                changedFiles.UnionWith(m_SnapshotRepository.GetChangedFiles(currentSnapshot));
                currentSnapshot = m_SnapshotRepository.GetPrecedingSnapshot(currentSnapshot);
            }

            return changedFiles.ToArray();            
        }

        public string[] GetChangedFiles(string fromId, string toId)
        {
            var changedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            // load snapshots to ensure they exists
            var fromSnapshot = GetSnapshotDo(ParseId(fromId));
            var toSnapshot = GetSnapshotDo(ParseId(toId));

            if(fromSnapshot.SequenceNumber >= toSnapshot.SequenceNumber)
                throw new InvalidRangeException($"Snapshot {toId} is not an descendant of {fromId}");

            // iterate over all snpshot and get the changed files for every item
            var currentSnapshot = toSnapshot;            
            while (currentSnapshot.SequenceNumber > fromSnapshot.SequenceNumber)
            {
                changedFiles.UnionWith(m_SnapshotRepository.GetChangedFiles(currentSnapshot));                
                currentSnapshot = m_SnapshotRepository.GetPrecedingSnapshot(currentSnapshot);
            }

            return changedFiles.ToArray();
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
            return (m_SnapshotRepository.GetPrecedingSnapshot(snapshotDo)?.Id)?.ToString();
        }


        static int ParseId(string id)
        {
            if(String.IsNullOrWhiteSpace(id))
                throw new ArgumentNullException(nameof(id), "Value cannot be null or whitespace");

            if (!int.TryParse(id, out var result))
            {
                throw new SnapshotNotFoundException(id);
            }

            return result;
        }

        FileSystemSnapshotDo GetSnapshotDo(int id) => 
            m_SnapshotRepository.GetSnapshotOrDefault(m_HistoryDo.Id, id) ?? throw new SnapshotNotFoundException(id.ToString());
        
    }
}
