using System.IO;
using Autofac;
using LibGit2Sharp;
using SyncTool.Common.Groups;
using SyncTool.FileSystem.TestHelpers;
using SyncTool.Git.Common.Groups;
using SyncTool.Git.RepositoryAccess;
using Xunit;

namespace SyncTool.Git.Test.Common.Groups
{
    public class GitGroupValidatorTest : DirectoryBasedTest
    {
        ILifetimeScope GetContainer()
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(new GroupStorage(m_TempDirectory.Location))
                .AsSelf();

            builder.RegisterType<GitGroupValidator>().AsSelf();

            return builder.Build();
        }

        [Fact]
        public void EnsureGroupIsValid_throws_ValidationException_when_the_address_does_not_point_to_a_git_repository()
        {            
            using (var container = GetContainer())
            {
                var instance = container.Resolve<GitGroupValidator>();
                Assert.Throws<GroupValidationException>(() => instance.EnsureGroupIsValid("Group1", "Address1"));
            }
        }

        [Fact]
        public void EnsureGroupIsValid_throws_ValidationException_when_the_address_does_not_point_to_a_group_git_repository()
        {            
            using (var container = GetContainer())
            using (var remote = CreateTemporaryDirectory())
            {
                var instance = container.Resolve<GitGroupValidator>();
                Repository.Init(remote.Location, true);
                Assert.Throws<GroupValidationException>(() => instance.EnsureGroupIsValid("Group1", remote.Location));
            }
        }

        [Fact]
        public void EnsureGroupIsValid_succeeds_for_a_repository_created_by_RepositoryInitHelper()
        {            
            using (var container = GetContainer())
            using (var remote = CreateTemporaryDirectory())
            {
                var instance = container.Resolve<GitGroupValidator>();

                RepositoryInitHelper.InitializeRepository(remote.Location);

                instance.EnsureGroupIsValid("Group1", remote.Location);

                // validation should not leave behind any data in the local directory
                Assert.Empty(Directory.EnumerateFileSystemEntries(m_TempDirectory.Location));
            }
        }
    }
}
