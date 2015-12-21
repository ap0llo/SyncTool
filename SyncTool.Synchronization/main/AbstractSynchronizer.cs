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
        

        void ProcessChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {            
            // case 1: local change only
            if (change.LeftChange == null && change.RightChange != null)
            {
                ProcessRightOnlyChange(leftChanges, rightChanges, change);
            }
            // case 2: global change only
            else if (change.LeftChange != null && change.RightChange != null)
            {
                ProcessLeftOnlyChange(leftChanges, rightChanges, change);       
            }
            // case 3: local and global change
            else if (change.LeftChange != null && change.RightChange != null)
            {
                ProcessDoubleChange(leftChanges, rightChanges, change);
            }
            else
            {
                throw new InvalidOperationException($"Encounted {nameof(GroupedChange)} in invalid state");
            }
        }


        void ProcessRightOnlyChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // file changed only in the local state

            switch (change.RightChange.Type)
            {
                // case 1: file was added locally
                case ChangeType.Added:
                    ProcessRightOnlyAddition(leftChanges, rightChanges, change);
                    break;

                // file was deleted locally
                case ChangeType.Deleted:
                    ProcessRightOnlyDeletion(leftChanges, rightChanges, change);
                    break;
                
                // case 3: file was modified locally
                case ChangeType.Modified:
                    ProcessRightOnlyModification(leftChanges, rightChanges, change);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ProcessRightOnlyModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            var globalState = leftChanges.ToSnapshot.RootDirectory;

            // case 1: file does not exist in the global state
            // => if this happens, something is seriously wrong
            if (globalState.FileExists(change.FilePath) == false)
            {
                throw new InvalidOperationException();
            }

            var globalFile = globalState.GetFile(change.FilePath);

            // case 2: file now matches the file in the global state 
            if (m_FileComparer.Equals(globalFile, change.RightChange.ToFile))
            {
                // => nothing to do, we're in sync
                return;
            }

            // case 3: file matched the global state prior to modification and was modified 
            if (m_FileComparer.Equals(globalFile, change.RightChange.FromFile))
            {
                // => apply local modification to global state
                throw new NotImplementedException();

            }
            // case 4: file was different from global state prior to modification and is different from global state now
            else
            {
                // => conflict                
                throw new NotImplementedException();
            }        
        }


        void ProcessRightOnlyAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {            
            var globalState = leftChanges.ToSnapshot.RootDirectory;

            // case 1: file also exists in the global state
            if (globalState.FileExists(change.FilePath))
            {
                var globalFile = globalState.GetFile(change.FilePath);
                // case 1.1: file in global state is identical to local file
                if(m_FileComparer.Equals(globalFile, change.RightChange.ToFile))
                {
                    throw new NotImplementedException();                    
                }
                // case 1.2: different file exists in the global state
                else
                {
                    // => conflict
                    throw new NotImplementedException();                   
                }
            }
            // case 2: file is not present in the global state
            else
            {
                // add file to global state
                throw new NotImplementedException();
            }            
        }

        void ProcessRightOnlyDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            var globalState = leftChanges.ToSnapshot.RootDirectory;

            // case 1: file does not exist in the global state 
            if (!globalState.FileExists(change.FilePath))
            {
                // => nothing to do
                return;
            }
            // case 2: file exists in the global state
            else
            {
                var globalFile = globalState.GetFile(change.FilePath);                
                // case 2.1: file was identical to global state prior to deletion
                if (m_FileComparer.Equals(globalFile, change.RightChange.FromFile))
                {
                    // => apply local change to global state
                    throw new NotImplementedException();                    
                }
                // case 2.2: a file different from the locally deleted file is present in the global state
                else
                {
                    // => conflict
                    throw new NotImplementedException();                    
                }
            }
        }


        void ProcessLeftOnlyChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            switch (change.LeftChange.Type)
            {
                case ChangeType.Added:
                    ProcessLeftOnlyAddition(leftChanges, rightChanges, change);
                    break;
                case ChangeType.Deleted:
                    ProcessLeftOnlyDeletion(leftChanges, rightChanges, change);
                    break;
                case ChangeType.Modified:
                    ProcessLeftOnlyModification(leftChanges, rightChanges, change);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ProcessLeftOnlyModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            var localState = rightChanges.ToSnapshot.RootDirectory;

            // case 1: file does not exist locally
            if (!localState.FileExists(change.FilePath))
            {
                // => if this happens, something is seriously wrong
                throw new InvalidOperationException();
            }

            var localFile = localState.GetFile(change.FilePath);

            // case 2: local file exists that matches the global file after the modification
            if (m_FileComparer.Equals(localFile, change.LeftChange.ToFile))
            {
                // => nothing to do
                return;
            }

            // case 3: local file exists that matches the global file prior to the modification
            if (m_FileComparer.Equals(localFile, change.LeftChange.FromFile))
            {
                // => apply global change to local state
                throw new NotImplementedException();
            }
            // case 4: local file exists but is different from global state both before and after the modification
            else
            {                
                // => conflict
                throw new NotImplementedException();
            }            
        }

        void ProcessLeftOnlyAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: identical file is also present locally

            // case 2: file is not present locally
            // => apply global change locally

            // case 3: different file exists locally
            // => conflict

            throw new NotImplementedException();
        }

        void ProcessLeftOnlyDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: file does not exist locally

            // case 2: file that was deleted globally exists locally
            // => apply global change to local filesystem

            // case 3: file different from the one deleted from global state exists locally
            // => conflict

            throw new NotImplementedException();
        }


        void ProcessDoubleChange(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Added)
            {
                ProcessDoubleAddition(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Modified)
            {
                ProcessAdditionAndModification(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Added && change.RightChange.Type == ChangeType.Deleted)
            {
                ProcessAdditionAndDeletion(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Added)
            {
                ProcessModificationAndAddition(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Modified)
            {
                ProcessDoubleModification(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Modified && change.RightChange.Type == ChangeType.Deleted)
            {
                ProcessModificationAndDeletion(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Added)
            {
                ProcessDeletionAndAddition(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Modified)
            {
                ProcessDeletionAndModification(leftChanges, rightChanges, change);
            }

            if (change.LeftChange.Type == ChangeType.Deleted && change.RightChange.Type == ChangeType.Deleted)
            {
                ProcessDoubleDeletion(leftChanges, rightChanges, change);
            }
        }

        void ProcessDoubleAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // case 1: the same file was added to both global and local states
            if (m_FileComparer.Equals(change.LeftChange.ToFile, change.RightChange.ToFile))
            {
                // nothing to do, states are in sync
            }
            // case 2: different files were added   
            else
            {
                throw new NotImplementedException();
            }
        }

        void ProcessAdditionAndModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessAdditionAndDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessModificationAndAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDoubleModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessModificationAndDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDeletionAndAddition(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDeletionAndModification(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDoubleDeletion(IFileSystemDiff leftChanges, IFileSystemDiff rightChanges, GroupedChange change)
        {
            // file is gone from both local and global states => nothing to do   
        }
    }
}