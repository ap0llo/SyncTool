// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using SyncTool.Common;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Local;
using SyncTool.Git.Common;
using SyncTool.Git.FileSystem;
using SyncTool.Synchronization.Conflicts;

namespace SyncTool.Git.Synchronization.Conflicts
{
    public class GitConflictService : GitBasedService, IConflictService
    {
        internal static readonly BranchName BranchName = new BranchName("synchronization", "conflicts");
        const string s_ConflictsDirectoryName = "Conflicts";
        

        public IEnumerable<ConflictInfo> Items
        {
            get
            {
                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    return Enumerable.Empty<ConflictInfo>();
                }

                var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);

                if (!root.DirectoryExists(s_ConflictsDirectoryName))
                {
                    return Enumerable.Empty<ConflictInfo>();
                }

                return LoadConflictInfos(root.GetDirectory(s_ConflictsDirectoryName));
            }            
        }

        public ConflictInfo this[string filePath]
        {
            get
            {
                PathValidator.EnsureIsValidFilePath(filePath);
                PathValidator.EnsureIsRootedPath(filePath);

                if (!GitGroup.Repository.LocalBranchExists(BranchName))
                {
                    throw new ItemNotFoundException($"There is no ConflictInfo for file '{filePath}'");
                }

                var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);

                var relativePath = GetRelativeConflictInfoFilePath(filePath);
                if (!root.FileExists(relativePath))
                {
                    throw new ItemNotFoundException($"There is no ConflictInfo for file '{filePath}'");
                }

                var file = (IReadableFile) root.GetFile(relativePath);
                return ConflictInfoFile.Load(null, file).Content;
            }
        }


        public GitConflictService(GitBasedGroup gitGroup) : base(gitGroup)
        {
        }

        
        public void AddItems(IEnumerable<ConflictInfo> conflicts)
        {
            if (conflicts == null)
            {
                throw new ArgumentNullException(nameof(conflicts));
            }
            conflicts = conflicts.ToArray();

            if(!conflicts.Any())
            {
                return;
            }

            EnsureBranchExists();

            var exisitngRoot = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);
            var createdRoot = new Directory(null, "root");

            // verify conflicts
            foreach (var conflict in conflicts)
            {                
                var relativePath = GetRelativeConflictInfoFilePath(conflict.FilePath);
                if (exisitngRoot.FileExists(relativePath))
                {
                    throw new DuplicateItemException($"A ConflictInfo for '{conflict.FilePath}' already exists");
                }

                var directory = DirectoryHelper.GetOrAddDirectory(createdRoot, PathParser.GetDirectoryName(relativePath));
                directory.Add(f => new ConflictInfoFile(f, conflict));
            }


            using (var workingDirectory = new TemporaryWorkingDirectory(GitGroup.Repository.Info.Path, BranchName.ToString()))
            {                
                var localItemCreator = new LocalItemCreator();
                localItemCreator.CreateDirectoryInPlace(createdRoot, workingDirectory.Location);

                workingDirectory.Commit($"{nameof(GitConflictService)}: Added {conflicts.Count()} items");
                workingDirectory.Push();
            }            

        }

        public void RemoveItems(IEnumerable<ConflictInfo> conflicts)
        {
            if (conflicts == null)
            {
                throw new ArgumentNullException(nameof(conflicts));
            }

            conflicts = conflicts.ToArray();

            if (!conflicts.Any())
            {
                return;
            }

            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                throw new ItemNotFoundException($"There is no ConflictInfo for file '{conflicts.First().FilePath}'");
            }


            var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);

            // verify conflicts
            foreach (var conflict in conflicts)
            {             
                var relativePath = GetRelativeConflictInfoFilePath(conflict.FilePath);
                if (!root.FileExists(relativePath))
                {
                    throw new ItemNotFoundException($"There is no ConflictInfo for file '{conflict.FilePath}'");
                }
            }


            // delete conflict info files
            using (var workingDirectory = new TemporaryWorkingDirectory(GitGroup.Repository.Info.Path, BranchName.ToString()))
            {
                var localDirectory= new LocalDirectory(null, workingDirectory.Location);

                foreach (var conflict in conflicts)
                {
                    var relativePath = GetRelativeConflictInfoFilePath(conflict.FilePath);
                    System.IO.File.Delete(((ILocalFile)localDirectory.GetFile(relativePath)).Location);
                }

                workingDirectory.Commit($"{nameof(GitConflictService)}: Removed {conflicts.Count()} items");
                workingDirectory.Push();
            }                
        }

        public bool ItemExists(string filePath)
        {
            PathValidator.EnsureIsValidFilePath(filePath);
            PathValidator.EnsureIsRootedPath(filePath);
            
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                return false;
            }

            var root = new GitDirectory(null, "root", GitGroup.Repository.GetLocalBranch(BranchName).Tip);
            return root.FileExists(GetRelativeConflictInfoFilePath(filePath));
        }

        
        IEnumerable<ConflictInfo> LoadConflictInfos(IDirectory directory)
        {
            return directory
                .EnumerateFilesRecursively()
                .Where(f => f.Name.EndsWith(ConflictInfoFile.FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
                .Cast<IReadableFile>()
                .Select(file => ConflictInfoFile.Load(null, file).Content);            
        }
        
        string GetRelativeConflictInfoFilePath(string filePath)
        {
            // assumes that filePath is rooted
            return s_ConflictsDirectoryName + filePath + ConflictInfoFile.FileNameSuffix;
        }

        /// <summary>
        /// Creates the sync action branch in the underlying repository if it does not exist yet
        /// </summary>
        void EnsureBranchExists()
        {
            if (!GitGroup.Repository.LocalBranchExists(BranchName))
            {
                var initalCommit = GitGroup.Repository.GetInitialCommit();
                GitGroup.Repository.CreateBranch(BranchName, initalCommit);
            }
        }
    }
}