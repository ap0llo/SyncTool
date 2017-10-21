using System;
using System.Collections.Generic;
using SyncTool.FileSystem;
using System.Linq;
using SyncTool.Utilities;

namespace SyncTool.Synchronization.State
{
    public sealed class SyncConflict : IEquatable<SyncConflict>
    {

        public string SnapshotId { get; }

        public string Path => ConflictingVersions.First(x => x != null).Path;

        public IReadOnlyList<FileReference> ConflictingVersions { get; }


        public SyncConflict(string snapshotId, IEnumerable<FileReference> conflictingVersions)
        {
            if (String.IsNullOrWhiteSpace(snapshotId))
                throw new ArgumentException("Value must not be empty or whitespace", nameof(snapshotId));
         
            if (conflictingVersions == null)
                throw new ArgumentNullException(nameof(conflictingVersions));

            var _conflictingVersions = conflictingVersions.ToArray();

            if (_conflictingVersions.Length < 2)
                throw new ArgumentException("Sync conflict needs to contains at least two conflicting versions", nameof(conflictingVersions));

            SnapshotId = snapshotId;
            ConflictingVersions = _conflictingVersions;
        }


        public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Path);

        public override bool Equals(object obj) => Equals(obj as SyncConflict);

        public bool Equals(SyncConflict other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.Ordinal.Equals(SnapshotId, other.SnapshotId) &&
                   ConflictingVersions.SetEqual(other.ConflictingVersions);
        }
    }
}
