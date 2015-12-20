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


        public void Synchronize(IFileSystemDiff globalChanges, IFileSystemDiff localChanges)
        {
            if (globalChanges == null)
            {
                throw new ArgumentNullException(nameof(globalChanges));
            }
            if (localChanges == null)
            {
                throw new ArgumentNullException(nameof(localChanges));
            }

            globalChanges = new FilteredFileSystemDiff(globalChanges, m_FileComparer);
            localChanges = new FilteredFileSystemDiff(localChanges, m_FileComparer);

            var combinedChanges = CombineChanges(globalChanges, localChanges).ToList();
            foreach (var change in combinedChanges)
            {
                ProcessChange(globalChanges, localChanges, change);
            }            
        }




        IEnumerable<GroupedChange> CombineChanges(IFileSystemDiff globalChanges, IFileSystemDiff localChanges)
        {
            var allChanges = globalChanges.Changes
                .Select(c => new {IsGlobalChange = true, Change = c})
                .Union(localChanges.Changes
                            .Select(c => new {IsGlobalChange = false, Change = c}));

            var groupedChanges = allChanges
                .GroupBy(change => change.Change.Path)
                .Select(group => 
                    new GroupedChange(
                        group.SingleOrDefault(c => c.IsGlobalChange)?.Change,
                        group.SingleOrDefault(c => c.IsGlobalChange == false)?.Change));

            return groupedChanges;
        }
        

        void ProcessChange(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {            
            // case 1: local change only
            if (change.GlobalChange == null && change.LocalChange != null)
            {
                ProcessLocalOnlyChange(globalChanges, localChanges, change);
            }
            // case 2: global change only
            else if (change.GlobalChange != null && change.LocalChange != null)
            {
                ProcessGlobalOnlyChange(globalChanges, localChanges, change);       
            }
            // case 3: local and global change
            else if (change.GlobalChange != null && change.LocalChange != null)
            {
                ProcessDoubleChange(globalChanges, localChanges, change);
            }
            else
            {
                throw new InvalidOperationException($"Encounted {nameof(GroupedChange)} in invalid state");
            }
        }


        void ProcessLocalOnlyChange(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            // file changed only in the local state

            switch (change.LocalChange.Type)
            {
                // case 1: file was added locally
                case ChangeType.Added:
                    ProcessLocalOnlyAddition(globalChanges, localChanges, change);
                    break;

                // file was deleted locally
                case ChangeType.Deleted:
                    ProcessLocalOnlyDeletion(globalChanges, localChanges, change);
                    break;
                
                // case 3: file was modified locally
                case ChangeType.Modified:
                    ProcessLocalOnlyModification(globalChanges, localChanges, change);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ProcessLocalOnlyModification(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            var globalState = globalChanges.ToSnapshot.RootDirectory;

            // case 1: file does not exist in the global state
            // => if this happens, something is seriously wrong
            if (globalState.FileExists(change.FilePath) == false)
            {
                throw new InvalidOperationException();
            }

            var globalFile = globalState.GetFile(change.FilePath);

            // case 2: file now matches the file in the global state 
            if (m_FileComparer.Equals(globalFile, change.LocalChange.ToFile))
            {
                // => nothing to do, we're in sync
                return;
            }

            // case 3: file matched the global state prior to modification and was modified 
            if (m_FileComparer.Equals(globalFile, change.LocalChange.FromFile))
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


        void ProcessLocalOnlyAddition(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {            
            var globalState = globalChanges.ToSnapshot.RootDirectory;

            // case 1: file also exists in the global state
            if (globalState.FileExists(change.FilePath))
            {
                var globalFile = globalState.GetFile(change.FilePath);
                // case 1.1: file in global state is identical to local file
                if(m_FileComparer.Equals(globalFile, change.LocalChange.ToFile))
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

        void ProcessLocalOnlyDeletion(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            var globalState = globalChanges.ToSnapshot.RootDirectory;

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
                if (m_FileComparer.Equals(globalFile, change.LocalChange.FromFile))
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


        void ProcessGlobalOnlyChange(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            switch (change.GlobalChange.Type)
            {
                case ChangeType.Added:
                    ProcessGlobalOnlyAddition(globalChanges, localChanges, change);
                    break;
                case ChangeType.Deleted:
                    ProcessGlobalOnlyDeletion(globalChanges, localChanges, change);
                    break;
                case ChangeType.Modified:
                    ProcessGlobalOnlyModification(globalChanges, localChanges, change);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ProcessGlobalOnlyModification(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            var localState = localChanges.ToSnapshot.RootDirectory;

            // case 1: file does not exist locally
            if (!localState.FileExists(change.FilePath))
            {
                // => if this happens, something is seriously wrong
                throw new InvalidOperationException();
            }

            var localFile = localState.GetFile(change.FilePath);

            // case 2: local file exists that matches the global file after the modification
            if (m_FileComparer.Equals(localFile, change.GlobalChange.ToFile))
            {
                // => nothing to do
                return;
            }

            // case 3: local file exists that matches the global file prior to the modification
            if (m_FileComparer.Equals(localFile, change.GlobalChange.FromFile))
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

        void ProcessGlobalOnlyAddition(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            // case 1: identical file is also present locally

            // case 2: file is not present locally
            // => apply global change locally

            // case 3: different file exists locally
            // => conflict

            throw new NotImplementedException();
        }

        void ProcessGlobalOnlyDeletion(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            // case 1: file does not exist locally

            // case 2: file that was deleted globally exists locally
            // => apply global change to local filesystem

            // case 3: file different from the one deleted from global state exists locally
            // => conflict

            throw new NotImplementedException();
        }


        void ProcessDoubleChange(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            if (change.GlobalChange.Type == ChangeType.Added && change.LocalChange.Type == ChangeType.Added)
            {
                ProcessDoubleAddition(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Added && change.LocalChange.Type == ChangeType.Modified)
            {
                ProcessAdditionAndModification(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Added && change.LocalChange.Type == ChangeType.Deleted)
            {
                ProcessAdditionAndDeletion(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Modified && change.LocalChange.Type == ChangeType.Added)
            {
                ProcessModificationAndAddition(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Modified && change.LocalChange.Type == ChangeType.Modified)
            {
                ProcessDoubleModification(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Modified && change.LocalChange.Type == ChangeType.Deleted)
            {
                ProcessModificationAndDeletion(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Deleted && change.LocalChange.Type == ChangeType.Added)
            {
                ProcessDeletionAndAddition(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Deleted && change.LocalChange.Type == ChangeType.Modified)
            {
                ProcessDeletionAndModification(globalChanges, localChanges, change);
            }

            if (change.GlobalChange.Type == ChangeType.Deleted && change.LocalChange.Type == ChangeType.Deleted)
            {
                ProcessDoubleDeletion(globalChanges, localChanges, change);
            }
        }

        void ProcessDoubleAddition(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            // case 1: the same file was added to both global and local states
            if (m_FileComparer.Equals(change.GlobalChange.ToFile, change.LocalChange.ToFile))
            {
                // nothing to do, states are in sync
            }
            // case 2: different files were added   
            else
            {
                throw new NotImplementedException();
            }
        }

        void ProcessAdditionAndModification(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessAdditionAndDeletion(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessModificationAndAddition(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDoubleModification(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessModificationAndDeletion(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDeletionAndAddition(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDeletionAndModification(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            throw new NotImplementedException();
        }

        void ProcessDoubleDeletion(IFileSystemDiff globalChanges, IFileSystemDiff localChanges, GroupedChange change)
        {
            // file is gone from both local and global states => nothing to do   
        }
    }
}