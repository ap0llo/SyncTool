using LibGit2Sharp;
using SyncTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SyncTool.Common.Utilities;

namespace SyncTool.Git.Common
{
    public class GitGroupInitializer : IGroupInitializer
    {
        readonly IRepositoryPathProvider m_PathProvider;

        public GitGroupInitializer(IRepositoryPathProvider pathProvider)
        {
            m_PathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public void Initialize(string groupName, string address)
        {
            var localPath = m_PathProvider.GetRepositoryPath(groupName);

            if (Directory.Exists(localPath) && Directory.EnumerateFileSystemEntries(localPath).Any())
            {
                throw new InitializationException($"Cannot create repository for SyncGroup '{groupName}'. Directory already exists and is not empty");
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
