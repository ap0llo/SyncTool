﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using Functional.Option;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public class Synchronizer : ISynchronizer
    {
        readonly IEqualityComparer<IFile> m_FileComparer;

        public Synchronizer(IEqualityComparer<IFile> fileComparer)
        {
            if (fileComparer == null)
            {
                throw new ArgumentNullException(nameof(fileComparer));
            }
            m_FileComparer = fileComparer;
        }


        public ISyncActionSet Synchronize(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges)
        {
            if (leftChanges == null)
            {
                throw new ArgumentNullException(nameof(leftChanges));
            }
            if (rightChanges == null)
            {
                throw new ArgumentNullException(nameof(rightChanges));
            }

            leftChanges = new FilteredFileSystemDiff(leftChanges, m_FileComparer);
            rightChanges = new FilteredFileSystemDiff(rightChanges, m_FileComparer);

            var combinedChanges = CombineChanges(leftChanges, rightChanges).ToList();

            var actionSet = new SyncActionSet(m_FileComparer);
            foreach (var change in combinedChanges)
            {
                var actionOption = ProcessChange(leftChanges, rightChanges, change);
                if (actionOption.HasValue)
                {
                    actionSet.Add(actionOption.Value);
                }
            }

            return actionSet;
        }



        IEnumerable<GroupedChange> CombineChanges(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges)
        {
            var allChanges = leftChanges.Changes
                .Select(c => new {IsLeftChange = true, Change = c})
                .Union(rightChanges.Changes
                    .Select(c => new {IsLeftChange = false, Change = c}));

            var groupedChanges = allChanges
                .GroupBy(change => change.Change.Path)
                .Select(group =>
                    new GroupedChange(
                        group.SingleOrDefault(c => c.IsLeftChange)?.Change,
                        group.SingleOrDefault(c => c.IsLeftChange == false)?.Change));

            return groupedChanges;
        }


        Option<SyncAction> ProcessChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: right change only
            if (change.LeftChange == null && change.RightChange != null)
            {
                return ProcessSingleChange(leftChanges.ToSnapshot.RootDirectory, change.RightChange, SyncParticipant.Right);
            }
            // case 2: left change only
            else if (change.LeftChange != null && change.RightChange == null)
            {
                return ProcessSingleChange(rightChanges.ToSnapshot.RootDirectory, change.LeftChange, SyncParticipant.Left);
            }
            // case 3: right and left change
            else if (change.LeftChange != null && change.RightChange != null)
            {
                return ProcessDoubleChange(change);
            }
            else
            {
                throw new InvalidOperationException($"Encounted {nameof(GroupedChange)} in invalid state");
            }
        }


        Option<SyncAction> ProcessSingleChange(IDirectory unchangedDirectory, IChange change, SyncParticipant changedParticipant)
        {
            // file changed only on left or right side only

            switch (change.Type)
            {
                case ChangeType.Added:
                    return ProcessSingleAddition(unchangedDirectory, change, changedParticipant);

                case ChangeType.Deleted:
                    return ProcessSingleDeletion(unchangedDirectory, change, changedParticipant);

                case ChangeType.Modified:
                    return ProcessSingleModification(unchangedDirectory, change, changedParticipant);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Option<SyncAction> ProcessSingleModification(IDirectory unchangedDirectory, IChange change, SyncParticipant changedParticipant)
        {
            // case 1: file does not exist in the other directory
            // => if this happens, something is seriously wrong because if the file is a new file it should be an addition, not a modification
            if (unchangedDirectory.FileExists(change.Path) == false)
            {
                throw new InvalidOperationException();
            }

            var unchangedFile = unchangedDirectory.GetFile(change.Path);

            // case 2: modified file now matches the file in the unchanged directory
            if (m_FileComparer.Equals(unchangedFile, change.ToFile))
            {
                // => nothing to do, we're in sync                
                return Option.None;
            }

            // case 3: changed file matched the file from the unchanged directory prior to modification and was modified 
            if (m_FileComparer.Equals(unchangedFile, change.FromFile))
            {
                // => apply local modification to the other directory
                return new ReplaceFileSyncAction(target: changedParticipant.Invert(),
                    oldVersion: ExtractFileFromTree(unchangedFile),
                    newVersion: ExtractFileFromTree(change.ToFile));
            }           
            // case 4: file was different from other directory prior to modification and is different from the other directory now
            // => conflict                
            return new MultipleVersionConflictSyncAction(unchangedFile, change.ToFile)
            {
                Description = $"A file exists in {changedParticipant.Invert()}, but a different version of the file was modified in {changedParticipant}"
            };
           
        }

        Option<SyncAction> ProcessSingleAddition(IDirectory unchangedDirectory, IChange change, SyncParticipant changedParticipant)
        {
            // case 1: file also exists in the unchanged directory
            if (unchangedDirectory.FileExists(change.Path))
            {
                var unchangedFile = unchangedDirectory.GetFile(change.Path);
                // case 1.1: file in the other directory is identical to the added file
                if (m_FileComparer.Equals(unchangedFile, change.ToFile))
                {
                    // => nothing to do
                    return Option.None;
                }
                // case 1.2: different file exists in the other directory
                else
                {
                    // => conflict
                    return new MultipleVersionConflictSyncAction(
                        ExtractFileFromTree(unchangedFile),
                        ExtractFileFromTree(change.ToFile))
                    {
                        Description = $"A file was added to '{changedParticipant}' but a different file is already present in {changedParticipant.Invert()}"
                    };
                }
            }
            // case 2: file is not present in the other directory
            else
            {
                // => add file to the other directory
                return new AddFileSyncAction(changedParticipant.Invert(), ExtractFileFromTree(change.ToFile));
            }            
        }

        Option<SyncAction> ProcessSingleDeletion(IDirectory unchangedDirectory, IChange change, SyncParticipant changedParticipant)
        {
            // case 1: file does not exist in the unchanged directory
            if (!unchangedDirectory.FileExists(change.Path))
            {
                // => nothing to do
                return Option.None;
            }
            // case 2: file exists in the other directory
            else
            {
                var unchangedFile = unchangedDirectory.GetFile(change.Path);

                // case 2.1: deleted file was identical to file in the unchanged directory prior to deletion
                if (m_FileComparer.Equals(unchangedFile, change.FromFile))
                {
                    // => apply change to the other directory
                    return new RemoveFileSyncAction(changedParticipant.Invert(), ExtractFileFromTree(unchangedFile));                    
                }
                // case 2.2: a file different from the deleted file is present in the other directory state
                else
                {
                    // => conflict
                    return new MultipleVersionConflictSyncAction(
                        ExtractFileFromTree(unchangedFile),
                        ExtractFileFromTree(change.FromFile))
                    {
                        Description = $"File was deleted from {changedParticipant} but the version in  {changedParticipant.Invert()} is different from the deleted version"
                    };                    
                }
            }
        }



        Option<SyncAction> ProcessDoubleChange(GroupedChange change)
        {
            if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Added)
            {
                return ProcessDoubleAddition(change);
            }
            else if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessAdditionAndModification(
                    change.LeftChange, SyncParticipant.Left,
                    change.RightChange, SyncParticipant.Right);
            }
            else if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Deleted)
            {
                throw new InvalidOperationException($"Addition and Deletion should is not a valid change combination (File {change.FilePath})");                
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Added)
            {
                return ProcessAdditionAndModification(
                    change.RightChange, SyncParticipant.Right,
                    change.LeftChange, SyncParticipant.Left);
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessDoubleModification(change);
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Deleted)
            {
                return ProcessModificationAndDeletion(
                    change.LeftChange, SyncParticipant.Left,
                    change.RightChange, SyncParticipant.Right);
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Added)
            {
                throw new InvalidOperationException($"Addition and Deletion should is not a valid change combination (File {change.FilePath})");
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessModificationAndDeletion(
                    change.RightChange, SyncParticipant.Right,
                    change.LeftChange, SyncParticipant.Left);
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Deleted)
            {
                return ProcessDoubleDeletion(change);
            }
            else
            {
                throw new InvalidOperationException($"Unhandled combination of change types: {nameof(change.LeftChange)} = { change.LeftChange.Type}, {nameof(change.RightChange)} = {change.RightChange.Type}");
            }
        }

        Option<SyncAction> ProcessDoubleAddition(GroupedChange change)
        {
            // case 1: the same file was added to both global and local states
            if (m_FileComparer.Equals(change.LeftChange.ToFile, change.RightChange.ToFile))
            {
                // nothing to do, states are in sync
                return Option.None;
            }
            // case 2: different files were added   
            else
            {
                return new MultipleVersionConflictSyncAction(
                    ExtractFileFromTree(change.LeftChange.ToFile),
                    ExtractFileFromTree(change.RightChange.ToFile))
                {
                    Description = "Different with the same name were added on both sync participants"
                };
            }
        }

        Option<SyncAction> ProcessAdditionAndModification(IChange addition, SyncParticipant addedOn, IChange modification, SyncParticipant modifiedOn)
        {
            // if files on both sides are identical now, everthing's fine, nothing to do
            if (m_FileComparer.Equals(addition.ToFile, modification.ToFile))
            {
                return Option.None;
            }
            else if (m_FileComparer.Equals(addition.ToFile, modification.FromFile))
            {
                // previous version of the file was added to one of the folder while the file was modified in the other
                // => apply the modification to the other folder as well
                return new ReplaceFileSyncAction(addedOn,
                    ExtractFileFromTree(addition.ToFile),
                    ExtractFileFromTree(modification.ToFile));                
            }
            else
            {
                return new MultipleVersionConflictSyncAction(
                    ExtractFileFromTree(addition.ToFile),
                    ExtractFileFromTree(modification.ToFile))
                {
                    Description = $"File was added on {addedOn} but was modified on {modifiedOn}"
                };
            }            
        }

        Option<SyncAction> ProcessDoubleModification(GroupedChange change)
        {
            // case 1: modification resulted in consistent state => nothing to do
            if (m_FileComparer.Equals(change.LeftChange.ToFile, change.RightChange.ToFile))
            {
                return Option.None;
            }
            // case 2: one modification modifies the result of the other modification
            // in this case, the newer modification can be applied to the other side as well
            // case 2.1  Left.FromFile -> Left.ToFile / Right.FromFile -> Right.ToFile
            else if (m_FileComparer.Equals(change.LeftChange.ToFile, change.RightChange.FromFile))
            {
                return new ReplaceFileSyncAction(SyncParticipant.Left,
                    ExtractFileFromTree(change.RightChange.FromFile),
                    ExtractFileFromTree(change.RightChange.ToFile));
            }
            // case 2.2  Right.FromFile -> Right.ToFile / Left.FromFile -> Left.ToFile
            else if (m_FileComparer.Equals(change.RightChange.ToFile, change.LeftChange.FromFile))
            {
                return new ReplaceFileSyncAction(SyncParticipant.Right,
                    ExtractFileFromTree(change.LeftChange.FromFile),
                    ExtractFileFromTree(change.LeftChange.ToFile));
            }
            // case 3: "FromFile" is identical for both changes 
            else if(m_FileComparer.Equals(change.LeftChange.FromFile, change.RightChange.FromFile))
            {
                // => conflicting modifications
                return new MultipleVersionConflictSyncAction(
                    ExtractFileFromTree(change.LeftChange.ToFile),
                    ExtractFileFromTree(change.RightChange.ToFile));
            }
            else
            {
                throw new InvalidOperationException($"Double modification without common base version ({change.FilePath})");
            }            
        }

        Option<SyncAction> ProcessModificationAndDeletion(IChange modification, SyncParticipant modifiedOn, IChange deletion, SyncParticipant deletedOn)
        {
            // case 1: the version of the file that was deleted on one side was deleted on the other
            if (m_FileComparer.Equals(modification.FromFile, deletion.FromFile))
            {
                // conflict: we need to either delete the modified file or add the modified version to the side where the previous version was deleted
                return new ModificationDeletionConflictSyncAction(modification.ToFile, deletion.FromFile)
                {
                    Description = $"File was deleted on {deletedOn} but modified on {modifiedOn}"
                };
            }
            // case 2: version that was deleted on one side matches the modified version from the other
            else if (m_FileComparer.Equals(modification.ToFile, deletion.FromFile))
            {
                // => apply deletion to other directory
                return new RemoveFileSyncAction(modifiedOn, modification.ToFile);
            }
            else
            {
                // other cases: this should never happen, if it does, something is wrong
                throw new InvalidOperationException($"Modification and deletion of file '{modification.Path}' with no common version");
            }           
        }


        Option<SyncAction> ProcessDoubleDeletion(GroupedChange change)
        {
            // file is gone from both local and global states => nothing to do  
            return Option.None;
        }


        IFile ExtractFileFromTree(IFile file)
        {
            return file.WithParent(new NullDirectory(file.Parent));
        }

    }
}