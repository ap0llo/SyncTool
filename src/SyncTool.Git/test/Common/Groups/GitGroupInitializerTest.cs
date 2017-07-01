using System.IO;
using System.Linq;
using Autofac;
using LibGit2Sharp;
using SyncTool.Common.Groups;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.Git.Common.Groups;
using SyncTool.Git.RepositoryAccess;
using Xunit;

namespace SyncTool.Git.Test.Common.Groups
{
    public class GitGroupInitializerTest : DirectoryBasedTest
    {
        ILifetimeScope GetContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new GroupStorage(m_TempDirectory.Location))
                .AsSelf();

            builder.RegisterType<GitGroupInitializer>().AsSelf();

            return builder.Build();
        }

        [Fact]
        public void Initialize_Creates_a_repository_and_pushes_it_to_the_remote_repository()
        {
            
            using (var lifeTime = GetContainer())
            using (var remoteDirectory = CreateTemporaryDirectory())
            {
                // set up the "remote" repository
                Repository.Init(remoteDirectory.Location, true);

                var instance = lifeTime.Resolve<GitGroupInitializer>();

                // create a new group
                instance.Initialize("Group1", remoteDirectory.Location);

                // creation of groups should not leave behind anything
                Assert.False(Directory.Exists(m_TempDirectory.Location));

                // assert that the group was actually created in the remote repository
                using (var repository = new Repository(remoteDirectory.Location))
                {
                    Assert.Equal(2, repository.Branches.Count());
                    Assert.True(repository.LocalBranchExists(RepositoryInitHelper.ConfigurationBranchName));
                    Assert.NotNull(repository.Tags[RepositoryInitHelper.InitialCommitTagName]);
                }
                
            }

        }    
    }
}
