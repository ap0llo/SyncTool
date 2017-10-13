using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    public class SyncStateDo
    {
        public string SnapshotId { get; set; }

        public long Version { get; set; }

        public List<SyncActionDo> Actions { get;set; }

        public List<SyncConflictDo> Conflicts { get; set; }
    }
}
