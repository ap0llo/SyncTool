using System;
using System.Collections.Generic;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.State
{
    public sealed class SyncAction : ISyncAction
    {
        public string SnapshotId { get; }

        public string Path { get; }

        public IFileReference FromVersion { get; }

        public IFileReference ToVersion { get; }


        public SyncAction(string snapshotId, string path, IFileReference fromVersion, IFileReference toVersion)
        {
            if (String.IsNullOrWhiteSpace(snapshotId))
                throw new ArgumentException("Value must not be empty", nameof(snapshotId));

            if (String.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Value must not be empty", nameof(path));

            SnapshotId = snapshotId;
            Path = path;
            FromVersion = fromVersion;
            ToVersion = toVersion;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(SnapshotId) * 397;
                hash ^= StringComparer.OrdinalIgnoreCase.GetHashCode(Path);
                hash ^= FromVersion?.GetHashCode() ?? 0;
                hash ^= ToVersion?.GetHashCode() ?? 0;
                return hash;
            }
        }

        public override bool Equals(object obj) => Equals(obj as ISyncAction);

        public bool Equals(ISyncAction other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.Ordinal.Equals(SnapshotId, other.SnapshotId) &&
                StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) &&
                EqualityComparer<IFileReference>.Default.Equals(FromVersion, other.FromVersion) &&
                EqualityComparer<IFileReference>.Default.Equals(ToVersion, other.ToVersion);
        }
    }
}
