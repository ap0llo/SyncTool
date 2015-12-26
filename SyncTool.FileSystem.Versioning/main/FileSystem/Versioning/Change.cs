using System;

namespace SyncTool.FileSystem.Versioning
{
    public class Change : IChange
    {
        public Change(ChangeType type, IFile fromFile, IFile toFile)
        {
            AssertIsValidChange(type, fromFile, toFile);
            AssertPathsAreEqual(fromFile, toFile);

            Type = type;
            FromFile = fromFile;
            ToFile = toFile;
        }

        public string Path => FromFile?.Path ?? ToFile.Path;

        public ChangeType Type { get; }

        public IFile FromFile { get; }

        public IFile ToFile { get; }


        void AssertIsValidChange(ChangeType type, IFile fromFile, IFile toFile)
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

        void AssertIsValidAddedChange(IFile fromFile, IFile toFile)
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

        void AssertIsValidDeletedChange(IFile fromFile, IFile toFile)
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

        void AssertIsValidModifiedChange(IFile fromFile, IFile toFile)
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

        void AssertPathsAreEqual(IFile fromFile, IFile toFile)
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