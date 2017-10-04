using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    public class MultiFileSystemSnapshotDo
    {
        public int Id { get; set; }

        public long CreationUnixTimeTicks { get; set; }

        public List<(string historyName, string snapshotId)> SnapshotIds { get; set; }
    }
}
