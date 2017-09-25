using SyncTool.FileSystem.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Sql.Model;
using SyncTool.Utilities;
using NodaTime;

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
            var toSnapshot = GetSnapshotDo(ParseId(toId));

            foreach (var snapshot in m_SnapshotRepository.GetSnapshotRange(toSnapshot))
            {
                changedFiles.UnionWith(m_SnapshotRepository.GetChangedFiles(snapshot));
            }
            return changedFiles.ToArray();            
        }

        public string[] GetChangedFiles(string fromId, string toId)
        {
            var changedFiles = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var fromSnapshot = GetSnapshotDo(ParseId(fromId));
            var toSnapshot = GetSnapshotDo(ParseId(toId));

            if (fromSnapshot.Id >= toSnapshot.Id)
                throw new InvalidRangeException($"Snapshot {toId} is not a descendant of {fromId}");

            foreach (var snapshot in m_SnapshotRepository.GetSnapshotRange(fromSnapshot, toSnapshot))
            {
                changedFiles.UnionWith(m_SnapshotRepository.GetChangedFiles(snapshot));
            }            
            return changedFiles.ToArray();
        }

        public IFileSystemDiff GetChanges(string toId, string[] pathFilter = null)
        {
            var toDbId = ParseId(toId);
            AssertIsValidPathFilter(pathFilter);

            var toSnapshot = GetSnapshotDo(toDbId);

            // iterate over all snapshots and get the changed files for every item
            var changeLists = new Dictionary<string, LinkedList<IChange>>(StringComparer.InvariantCultureIgnoreCase);            
            foreach (var snapshot in m_SnapshotRepository.GetSnapshotRange(toSnapshot))
            {
                AppendChanges(snapshot, changeLists, pathFilter);
            }

            return new FileSystemDiff(
                this, 
                m_SnapshotFactory.Invoke(this, toSnapshot),                 
                changeLists.Values.Select(x => new ChangeList(x)));
        }
        
        public IFileSystemDiff GetChanges(string fromId, string toId, string[] pathFilter = null)
        {
            var fromDbId = ParseId(fromId);
            var toDbId = ParseId(toId);
            AssertIsValidPathFilter(pathFilter);

            var fromSnapshot = GetSnapshotDo(fromDbId);
            var toSnapshot = GetSnapshotDo(toDbId);

            if (fromSnapshot.Id >= toSnapshot.Id)
                throw new InvalidRangeException($"Snapshot {toId} is not a descendant of {fromId}");

            // iterate over all snapshots and get the changed files for every item
            var changeLists = new Dictionary<string, LinkedList<IChange>>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var snapshot in m_SnapshotRepository.GetSnapshotRange(fromSnapshot, toSnapshot))
            {
                AppendChanges(snapshot, changeLists, pathFilter);
            }

            return new FileSystemDiff(
                this,
                m_SnapshotFactory.Invoke(this, fromSnapshot),
                m_SnapshotFactory.Invoke(this, toSnapshot),
                changeLists.Values.Select(x => new ChangeList(x)));
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

        static IFileReference GetFileReference(FileInstanceDo instanceDo)
            => new FileReference(
                path: instanceDo.File.Path,
                lastWriteTime: Instant.FromUnixTimeTicks(instanceDo.LastWriteUnixTimeTicks),
                length: instanceDo.Length);
        
        void AppendChanges(FileSystemSnapshotDo currentSnapshot, Dictionary<string, LinkedList<IChange>> changeLists, string[] pathFilter)
        {            
            foreach (var (previous, current) in m_SnapshotRepository.GetChanges(currentSnapshot, pathFilter))
            {
                var change = GetChange(previous, current);
                
                changeLists
                    .GetOrAdd(change.Path, () => new LinkedList<IChange>())
                    .AddLast(change);
            }
        }

        static IChange GetChange(FileInstanceDo previous, FileInstanceDo current)
        {
            if (previous == null && current != null)
                return new Change(ChangeType.Added, null, GetFileReference(current));
            else if (previous != null && current == null)
                return new Change(ChangeType.Deleted, GetFileReference(previous), null);
            else if (previous != null && current != null)
                return new Change(ChangeType.Modified, GetFileReference(previous), GetFileReference(current));
            else
                throw new InvalidOperationException();            
        }

        void AssertIsValidPathFilter(string[] pathFilter)
        {
            if (pathFilter == null)
            {
                return;
            }

            foreach (var path in pathFilter)
            {
                PathValidator.EnsureIsValidFilePath(path);
                PathValidator.EnsureIsRootedPath(path);
            }
        }
    }
}
