using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Default, immutable implementation of <see cref="IChange"/>
    /// </summary>
    public class Change : IChange
    {         
        public string Path => FromVersion?.Path ?? ToVersion.Path;

        public ChangeType Type { get; }
        
        public FileReference FromVersion { get; }

        public FileReference ToVersion { get; }


        public Change(ChangeType type, FileReference fromFile, FileReference toFile)
        {
            AssertIsValidChange(type, fromFile, toFile);
            AssertPathsAreEqual(fromFile, toFile);

            Type = type;
            FromVersion = fromFile;
            ToVersion = toFile;
        }


        public bool Equals(IChange other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.InvariantCultureIgnoreCase.Equals(Path, other.Path) &&
                   Type == other.Type &&
                   EqualityComparer<FileReference>.Default.Equals(FromVersion, other.FromVersion) &&
                   EqualityComparer<FileReference>.Default.Equals(ToVersion, other.ToVersion);
        }

        public override bool Equals(object obj) => Equals(obj as IChange);

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path) |
                   Type.GetHashCode() |
                   (FromVersion?.GetHashCode() ?? 0) |
                   (ToVersion?.GetHashCode() ?? 0);
        }


        void AssertIsValidChange(ChangeType type, FileReference fromFile, FileReference toFile)
        {
            switch (type)
            {
                case ChangeType.Added:
                    AssertIsValidAddedChange(fromFile, toFile);
                    break;
                case ChangeType.Deleted:
                    AssertIsValidDeletedChange(fromFile, toFile);
                    break;
                case ChangeType.Modified:
                    AssertIsValidModifiedChange(fromFile, toFile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        void AssertIsValidAddedChange(FileReference fromFile, FileReference toFile)
        {
            if (fromFile != null)
            {
                throw new ArgumentException($"{nameof(fromFile)} must be null for ChangeType {ChangeType.Added}", nameof(fromFile));
            }
            if (toFile == null)
            {
                throw new ArgumentNullException(nameof(toFile));
            }
        }

        void AssertIsValidDeletedChange(FileReference fromFile, FileReference toFile)
        {
            if (fromFile == null)
            {
                throw new ArgumentNullException(nameof(fromFile));
            }
            if (toFile != null)
            {
                throw new ArgumentException($"{nameof(toFile)} must be null for ChangeType {ChangeType.Deleted}", nameof(toFile));
            }
        }

        void AssertIsValidModifiedChange(FileReference fromFile, FileReference toFile)
        {
            if (fromFile == null)
            {
                throw new ArgumentNullException(nameof(fromFile));
            }
            if (toFile == null)
            {
                throw new ArgumentNullException(nameof(toFile));
            }
        }

        void AssertPathsAreEqual(FileReference fromFile, FileReference toFile)
        {
            if (fromFile == null || toFile == null)
            {
                return;
            }

            if (!StringComparer.InvariantCultureIgnoreCase.Equals(fromFile.Path, toFile.Path))
            {
                throw new ArgumentException($"Path differs between {nameof(fromFile)} and {nameof(toFile)}");
            }
        }
    }
}