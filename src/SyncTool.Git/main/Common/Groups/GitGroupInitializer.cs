using System;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using SyncTool.Common.Groups;
using SyncTool.Git.RepositoryAccess;
using SyncTool.Utilities;

namespace SyncTool.Git.Common.Groups
{
    public class GitGroupInitializer : IGroupInitializer
    {
        readonly GroupStorage m_GroupStorage;

        public GitGroupInitializer(GroupStorage groupStorage)
        {
            m_GroupStorage = groupStorage ?? throw new ArgumentNullException(nameof(groupStorage));
        }

        public void Initialize(string groupName, string address)
        {
            var localPath = m_GroupStorage.Path;

            if (Directory.Exists(localPath) && Directory.EnumerateFileSystemEntries(localPath).Any())
            {
                throw new GroupInitializationException($"Cannot create repository for SyncGroup '{groupName}'. Directory already exists and is not empty");
            }

            Directory.CreateDirectory(localPath);

            RepositoryInitHelper.InitializeRepository(localPath);

            using (var repository = new Repository(localPath))
            {
                var origin = repository.Network.Remotes.Add("origin", address);

                foreach (var localBranch in repository.GetLocalBranches())
                {
                    repository.Branches.Update(localBranch,
                            b => b.Remote = origin.Name,
                            b => b.UpstreamBranch = localBranch.CanonicalName);

                }
                repository.Network.Push(origin, repository.Branches.GetLocalBranches().ToRefSpecs().Union(repository.Tags.ToRefSpecs()));
            }

            DirectoryHelper.DeleteRecursively(localPath);

        }
    }
}
