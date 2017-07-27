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
    class SqlFileSystemHistory : IFileSystemHistory
    {
        private readonly FileSystemHistoryDo m_HistoryDo;

        public IFileSystemSnapshot this[string id] => throw new NotImplementedException();

        public string Name => m_HistoryDo.Name;

        public string Id => m_HistoryDo.Id.ToString();

        public IFileSystemSnapshot LatestFileSystemSnapshot => throw new NotImplementedException();

        public IEnumerable<IFileSystemSnapshot> Snapshots => throw new NotImplementedException();

        
        public SqlFileSystemHistory(FileSystemHistoryDo historyDo)
        {
            m_HistoryDo = historyDo ?? throw new ArgumentNullException(nameof(historyDo));
        }

        public IFileSystemSnapshot CreateSnapshot(IDirectory fileSystemState)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }
}
