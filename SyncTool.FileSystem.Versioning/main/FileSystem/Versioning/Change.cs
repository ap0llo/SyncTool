﻿// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Default, immutable implementation of <see cref="IChange"/>
    /// </summary>
    public class Change : IChange
    {        
        public Change(ChangeType type, IFile fromFile, IFile toFile) : this(type, fromFile?.ToReference(), toFile?.ToReference())
        {
            FromFile = fromFile;
            ToFile = toFile;
        }

        //This will become the new constructor, once the obsolete IFile properties are removed
        private Change(ChangeType type, IFileReference fromFile, IFileReference toFile)
        {
            AssertIsValidChange(type, fromFile, toFile);
            AssertPathsAreEqual(fromFile, toFile);

            Type = type;
            FromVersion = fromFile;
            ToVersion = toFile;            
        }

        public string Path => FromFile?.Path ?? ToFile.Path;

        public ChangeType Type { get; }

        public IFile FromFile { get; }

        public IFile ToFile { get; }

        public IFileReference FromVersion { get; }

        public IFileReference ToVersion { get; }

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