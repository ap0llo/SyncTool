using System;
using System.Collections.Generic;
using SyncTool.Sql.Model;
using SyncTool.Sql.TestHelpers;
using Xunit;
using System.Linq;

namespace SyncTool.Sql.Test.Model
{
    public class SnapshotRepositoryTest : SqlTestBase
    {
        readonly FileSystemHistoryRepository m_HistoryRepository;
        readonly FileSystemRepository m_FileSystemRepository;
        readonly SnapshotRepository m_Instance;

        readonly int m_HistoryId1;
        readonly int m_HistoryId2;
        
        public SnapshotRepositoryTest()
        {            
            m_HistoryRepository = new FileSystemHistoryRepository(Database);
            m_FileSystemRepository = new FileSystemRepository(Database);
            m_Instance = new SnapshotRepository(Database);

            var historyDo = new FileSystemHistoryDo("history1");
            historyDo = m_HistoryRepository.AddItem(historyDo);
            m_HistoryId1 = historyDo.Id;

            historyDo = new FileSystemHistoryDo("history2");
            historyDo = m_HistoryRepository.AddItem(historyDo);
            m_HistoryId2 = historyDo.Id;
        }


        [Fact]
        public void AddSnapshot_assigns_a_sequence_number()
        {
            var dir = new DirectoryInstanceDo(new DirectoryDo() { Name = "root", NormalizedPath = "" });
            m_FileSystemRepository.AddRecursively(dir);

            var snapshot1 = new FileSystemSnapshotDo(m_HistoryId1, DateTime.UtcNow.Ticks, dir.Id, new List<FileInstanceDo>());            
            var snapshot2 = new FileSystemSnapshotDo(m_HistoryId2, DateTime.UtcNow.Ticks, dir.Id, new List<FileInstanceDo>());            

            m_Instance.AddSnapshot(snapshot1);
            m_Instance.AddSnapshot(snapshot2);
            
            Assert.Equal(0, snapshot1.SequenceNumber);
            Assert.Equal(1, snapshot2.SequenceNumber);            
        }

        [Fact]
        public void AddSnapshot_saves_included_files()
        {
            var dir = new DirectoryInstanceDo(new DirectoryDo() { Name = "root", NormalizedPath = "" });
            var file1 = new FileInstanceDo(new FileDo() { Name = "file1", NormalizedPath = "/dir1/file1".NormalizeCaseInvariant(), Path = "/dir1/file1" }, DateTime.Now, 42);
            dir.Files.Add(file1);
            m_FileSystemRepository.AddRecursively(dir);

            var snapshot = new FileSystemSnapshotDo(m_HistoryId1, DateTime.UtcNow.Ticks, dir.Id, new List<FileInstanceDo>() { file1 });

            m_Instance.AddSnapshot(snapshot);
            var loadedSnapshot = m_Instance.GetSnapshotOrDefault(m_HistoryId1, snapshot.Id);

            m_Instance.LoadIncludedFiles(loadedSnapshot);
            Assert.NotNull(loadedSnapshot.IncludedFiles);
            Assert.Single(loadedSnapshot.IncludedFiles.Where(x => x.Id == file1.Id));
        }
    }
}