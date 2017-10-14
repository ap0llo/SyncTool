using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization.State
{    
    //TODO: Implementors should override ToString()
    //TODO: Add change type similar to "Change"
    public sealed class SyncAction : IEquatable<SyncAction>
    {
        [NotNull]
        public string SnapshotId { get; }

        public string Path => FromVersion?.Path ?? ToVersion.Path;
        
        public FileReference FromVersion { get; }

        public FileReference ToVersion { get; }


        public SyncAction(string snapshotId, FileReference fromVersion, FileReference toVersion)
        {
            if (String.IsNullOrWhiteSpace(snapshotId))
                throw new ArgumentException("Value must not be empty", nameof(snapshotId));
            
            SnapshotId = snapshotId;            
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

        public override bool Equals(object obj) => Equals(obj as SyncAction);

        public bool Equals(SyncAction other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.Ordinal.Equals(SnapshotId, other.SnapshotId) &&
                StringComparer.OrdinalIgnoreCase.Equals(Path, other.Path) &&
                EqualityComparer<FileReference>.Default.Equals(FromVersion, other.FromVersion) &&
                EqualityComparer<FileReference>.Default.Equals(ToVersion, other.ToVersion);
        }
    }
}
