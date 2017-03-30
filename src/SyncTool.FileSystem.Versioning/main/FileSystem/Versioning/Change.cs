// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
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
        
        public IFileReference FromVersion { get; }

        public IFileReference ToVersion { get; }


        public Change(ChangeType type, IFileReference fromFile, IFileReference toFile)
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
                   EqualityComparer<IFileReference>.Default.Equals(FromVersion, other.FromVersion) &&
                   EqualityComparer<IFileReference>.Default.Equals(ToVersion, other.ToVersion);
        }

        public override bool Equals(object obj) => Equals(obj as IChange);

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(Path) |
                   Type.GetHashCode() |
                   (FromVersion?.GetHashCode() ?? 0) |
                   (ToVersion?.GetHashCode() ?? 0);
        }


        void AssertIsValidChange(ChangeType type, IFileReference fromFile, IFileReference toFile)
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

        void AssertIsValidAddedChange(IFileReference fromFile, IFileReference toFile)
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

        void AssertIsValidDeletedChange(IFileReference fromFile, IFileReference toFile)
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

        void AssertIsValidModifiedChange(IFileReference fromFile, IFileReference toFile)
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

        void AssertPathsAreEqual(IFileReference fromFile, IFileReference toFile)
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