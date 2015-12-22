// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using SyncTool.FileSystem;

namespace SyncTool.Synchronization
{
    public class ResolvedSyncAction : SyncAction
    {

        public IFile OldValue { get; set; }

        public IFile NewValue { get; set; }

        public ResolvedSyncAction(SyncActionType type, IFile oldValue, IFile newValue) : base(type)
        {
            switch (type)
            {
                case SyncActionType.AddFile:
                    if (oldValue != null)
                    {
                        throw new ArgumentException(nameof(oldValue), $"Value must be null for {nameof(SyncActionType)}.{type}");
                    }
                    if (newValue == null)
                    {
                        throw new ArgumentNullException(nameof(newValue));
                    }
                    break;
                case SyncActionType.RemoveFile:
                    if (oldValue == null)
                    {
                        throw new ArgumentNullException(nameof(oldValue));
                    }
                    if (newValue != null)
                    {
                        throw new ArgumentException(nameof(newValue), $"Value must be null for {nameof(SyncActionType)}.{type}");
                    }
                    break;
                case SyncActionType.ReplaceFile:
                    if (oldValue == null)
                    {
                        throw new ArgumentNullException(nameof(oldValue));
                    }
                    if (newValue == null)
                    {
                        throw new ArgumentNullException(nameof(newValue));
                    }
                    break;
                case SyncActionType.ResolveConflict:
                    throw new ArgumentException($"{nameof(SyncActionType)} '{type}' cannot be used with {nameof(ResolvedSyncAction)}", nameof(type));

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            if (newValue != null && oldValue != null)
            {
                if (!StringComparer.InvariantCultureIgnoreCase.Equals(oldValue.Path, newValue.Path))
                {
                    throw new ArgumentException($"The paths of {nameof(oldValue)} and {nameof(newValue)} are differnet");
                }
            }

  
            this.OldValue = oldValue;
            this.NewValue = newValue;
        }
    }
}