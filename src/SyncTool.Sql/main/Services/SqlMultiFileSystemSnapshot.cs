using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Sql.Model;

namespace SyncTool.Sql.Services
{
    public class SqlMultiFileSystemSnapshot : IMultiFileSystemSnapshot
    {
        readonly IHistoryService m_HistoryService;
        readonly MultiFileSystemSnapshotRepository m_Repository;
        readonly MultiFileSystemSnapshotDo m_SnapshotDo;
        readonly Lazy<IDictionary<string, string>> m_SnapshotIds;


        public string Id => m_SnapshotDo.Id.ToString();

        public IEnumerable<string> HistoryNames => m_SnapshotIds.Value.Keys;


        public SqlMultiFileSystemSnapshot(
            [NotNull] IHistoryService historyService,
            [NotNull] MultiFileSystemSnapshotRepository repository, 
            [NotNull] MultiFileSystemSnapshotDo snapshotDo)
        {
            m_HistoryService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            m_Repository = repository ?? throw new ArgumentNullException(nameof(repository));
            m_SnapshotDo = snapshotDo ?? throw new ArgumentNullException(nameof(snapshotDo));

            m_SnapshotIds = new Lazy<IDictionary<string, string>>(() =>
            {
                m_Repository.LoadSnapshots(m_SnapshotDo);
                return m_SnapshotDo.SnapshotIds.ToDictionary(
                        x => x.historyName,                     
                        x => x.snapshotId, 
                        StringComparer.InvariantCultureIgnoreCase);
            });         
        }


        public IFileSystemSnapshot GetSnapshot(string historyName)
        {
            var snapshotId = m_SnapshotIds.Value[historyName];
            return snapshotId == null ? null : m_HistoryService[historyName][snapshotId];
        }

        public string GetSnapshotId(string historyName) => m_SnapshotIds.Value[historyName];

        public IEnumerable<(string historyName, IFile file)> GetFiles(string path)
        {
            foreach (var historyName in HistoryNames)
            {
                var rootDirectory = GetSnapshot(historyName).RootDirectory;
                var file = rootDirectory.GetFileOrDefault(path);
                yield return (historyName, file);
            }
        }
    }
}