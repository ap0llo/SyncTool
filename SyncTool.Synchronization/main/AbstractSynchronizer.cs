// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;

namespace SyncTool.Synchronization
{
    public abstract class AbstractSynchronizer : ISynchronizer
    {
        readonly IEqualityComparer<IFile> m_FileComparer;

        protected AbstractSynchronizer(IEqualityComparer<IFile> fileComparer)
        {
            if (fileComparer == null)
            {
                throw new ArgumentNullException(nameof(fileComparer));
            }
            m_FileComparer = fileComparer;
        }


        public void Synchronize(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges)
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
            foreach (var change in combinedChanges)
            {
                ProcessChange(leftChanges, rightChanges, change);
            }
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


        IEnumerable<SyncAction> ProcessChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: local change only
            if (change.LeftChange == null && change.RightChange != null)
            {
                return ProcessRightOnlyChange(leftChanges, rightChanges, change);
            }
            // case 2: global change only
            else if (change.LeftChange != null && change.RightChange != null)
            {
                return ProcessLeftOnlyChange(leftChanges, rightChanges, change);
            }
            // case 3: local and global change
            else if (change.LeftChange != null && change.RightChange != null)
            {
                return ProcessDoubleChange(leftChanges, rightChanges, change);
            }
            else
            {
                throw new InvalidOperationException($"Encounted {nameof(GroupedChange)} in invalid state");
            }
        }


        IEnumerable<SyncAction> ProcessLeftOnlyChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            switch (change.LeftChange.Type)
            {
                case ChangeType.Added:
                    return ProcessSingleAddition(rightChanges.ToSnapshot.RootDirectory, change.LeftChange);
                    
                case ChangeType.Deleted:
                    return ProcessSingleDeletion(rightChanges.ToSnapshot.RootDirectory, change.LeftChange);
                    
                case ChangeType.Modified:
                    return ProcessSingleModification(rightChanges.ToSnapshot.RootDirectory, change.LeftChange);
                    
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        IEnumerable<SyncAction> ProcessRightOnlyChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // file changed only in the local state

            switch (change.RightChange.Type)
            {
                // case 1: file was added locally
                case ChangeType.Added:
                    return ProcessSingleAddition(leftChanges.ToSnapshot.RootDirectory, change.RightChange);                    

                // file was deleted locally
                case ChangeType.Deleted:
                    return ProcessSingleDeletion(leftChanges.ToSnapshot.RootDirectory, change.RightChange);                    

                // case 3: file was modified locally
                case ChangeType.Modified:
                    return ProcessSingleModification(leftChanges.ToSnapshot.RootDirectory, change.RightChange);                    

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        IEnumerable<SyncAction> ProcessSingleModification(IDirectory unchangedDirectory, IChange change)
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
                yield break;
            }

            // case 3: changed file matched the file from the unchanged directory prior to modification and was modified 
            if (m_FileComparer.Equals(unchangedFile, change.FromFile))
            {
                // => apply local modification to global state
                yield return new ResolvedSyncAction(
                    type: SyncActionType.ReplaceFile,
                    newValue: change.ToFile.WithParent(new NullDirectory(change.ToFile.Parent)),
                    oldValue: unchangedFile.WithParent(new NullDirectory(unchangedFile.Parent)));
            }           
            // case 4: file was different from global state prior to modification and is different from global state now
            // => conflict                
            yield return new ConflictSyncAction(unchangedFile, change.ToFile);
           
        }

        IEnumerable<SyncAction> ProcessSingleAddition(IDirectory unchangedDirectory, IChange change)
        {
            // case 1: file also exists in the unchanged directory
            if (unchangedDirectory.FileExists(change.Path))
            {
                var unchangedFile = unchangedDirectory.GetFile(change.Path);
                // case 1.1: file in the other directory is identical to the added file
                if (m_FileComparer.Equals(unchangedFile, change.ToFile))
                {
                    throw new NotImplementedException();
                }
                    // case 1.2: different file exists in the other directory
                // => conflict
                throw new NotImplementedException();
            }
                // case 2: file is not present in the other directory
            // => add file to global state
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessSingleDeletion(IDirectory unchangedDirectory, IChange change)
        {
            // case 1: file does not exist in the unchanged directory
            if (!unchangedDirectory.FileExists(change.Path))
            {
                // => nothing to do
                return Enumerable.Empty<SyncAction>();
            }
            // case 2: file exists in the other directory
            else
            {
                var unchangedFile = unchangedDirectory.GetFile(change.Path);

                // case 2.1: deleted file was identical to file in the unchanged directory prior to deletion
                if (m_FileComparer.Equals(unchangedFile, change.FromFile))
                {
                    // => apply local change to global state
                    throw new NotImplementedException();
                }
                    // case 2.2: a file different from the deleted file is present in the other directory state
                // => conflict
                throw new NotImplementedException();
            }
        }



        IEnumerable<SyncAction> ProcessDoubleChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Added)
            {
                return ProcessDoubleAddition(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessAdditionAndModification(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Deleted)
            {
                return ProcessAdditionAndDeletion(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Added)
            {
                return ProcessModificationAndAddition(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessDoubleModification(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Deleted)
            {
                return ProcessModificationAndDeletion(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Added)
            {
                return ProcessDeletionAndAddition(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Modified)
            {
                return ProcessDeletionAndModification(leftChanges, rightChanges, change);
            }
            else if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Deleted)
            {
                return ProcessDoubleDeletion(leftChanges, rightChanges, change);
            }
            else
            {
                throw new InvalidOperationException($"Unhandled combination of change types: {nameof(change.LeftChange)} = { change.LeftChange.Type}, {nameof(change.RightChange)} = {change.RightChange.Type}");
            }
        }

        IEnumerable<SyncAction> ProcessDoubleAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: the same file was added to both global and local states
            if (m_FileComparer.Equals(change.LeftChange.ToFile, change.RightChange.ToFile))
            {
                // nothing to do, states are in sync
                return Enumerable.Empty<SyncAction>();
            }
            // case 2: different files were added   
            else
            {
                throw new NotImplementedException();
            }
        }

        IEnumerable<SyncAction>  ProcessAdditionAndModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessAdditionAndDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessModificationAndAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessDoubleModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessModificationAndDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessDeletionAndAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessDeletionAndModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        IEnumerable<SyncAction> ProcessDoubleDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // file is gone from both local and global states => nothing to do  
            return Enumerable.Empty<SyncAction>();
        }
    }
}