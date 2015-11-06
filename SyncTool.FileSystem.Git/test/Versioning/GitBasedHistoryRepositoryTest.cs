using System.Linq;
using LibGit2Sharp;
using SyncTool.FileSystem.Local;
using Xunit;

namespace SyncTool.FileSystem.Git
{
    public class GitBasedHistoryRepositoryTest : DirectoryBasedTest
    {


        [Fact]
        public void Create_can_create_a_new_repository()
        {
            using (var historyRepository = GitBasedHistoryRepository.Create(m_TempDirectory.Location))
            {
                Assert.Empty(historyRepository.Histories);                
            }
        }


        [Fact]
        public void CreateHistory_can_create_multiple_histories()
        {
            var historyNames = new[] {"history1", "histroy2"};

            using (var historyRepository = GitBasedHistoryRepository.Create(m_TempDirectory.Location))
            {

                foreach (var name in historyNames)
                {
                    historyRepository.CreateHistory(name);
                }

                Assert.Equal(historyNames.Length, historyRepository.Histories.Count());
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        [InlineData(100)]
        public void CreateHistory_creates_a_new_branch_in_the_underlying_git_repository(int numberOfHistoriesToCreate)
        {
            using (var historyRepository = GitBasedHistoryRepository.Create(m_TempDirectory.Location))
            {
                
                for (int i = 0; i < numberOfHistoriesToCreate; i++)
                {
                    historyRepository.CreateHistory($"history-{i}");
                }           
            }
            using (var repo = new Repository(m_TempDirectory.Location))
            {
                Assert.Equal(numberOfHistoriesToCreate + 1, repo.Branches.Count());                
            }

        }

    }
}