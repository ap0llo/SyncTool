using System;
using Autofac;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using NodaTime;
using Xunit;
using SyncTool.Common;
using SyncTool.Common.DI;
using SyncTool.Common.Groups;
using SyncTool.Common.Options;
using SyncTool.Common.TestHelpers;
using SyncTool.Configuration;
using SyncTool.FileSystem;
using SyncTool.FileSystem.DI;
using SyncTool.FileSystem.Versioning;
using SyncTool.FileSystem.Versioning.TestHelpers;
using SyncTool.Sql.DI;
using SyncTool.Sql.TestHelpers;
using SyncTool.Synchronization;
using SyncTool.Synchronization.DI;
using SyncTool.Utilities;
using System.Linq;

namespace SyncTool.Test
{

    public class SynchronizerIntegrationTest : SqlTestBase
    {
        readonly ISynchronizer m_Synchronizer;
        readonly TemporaryDirectory m_TempDirectory;
        readonly IContainer m_Container;
        readonly ILifetimeScope m_ApplicationScope;
        readonly IGroup m_Group;

        public SynchronizerIntegrationTest()
        {
            m_TempDirectory = new TemporaryDirectory();

            var dataOptions = new ApplicationDataOptions()
            {
                RootPath = m_TempDirectory
            };

            var containerBuilder = new ContainerBuilder();

            containerBuilder.RegisterGeneric(typeof(NullLogggerProxy<>)).As(typeof(ILogger<>));
            containerBuilder.RegisterInstance(NullLogger.Instance).As<ILogger>();

            containerBuilder.RegisterInstance(dataOptions).AsSelf();
            containerBuilder.RegisterModule<SqlModuleFactoryModule>();       // add support for database-backed groups
            containerBuilder.RegisterModule<CommonApplicationScopeModule>(); // SyncTool.Common
            containerBuilder.RegisterModule<FileSystemModule>();             // SyncTool.FileSystem
            containerBuilder.RegisterModule<SynchronizationModule>();        // SyncTool.Synchronization

            m_Container = containerBuilder.Build();
            m_ApplicationScope = m_Container.BeginLifetimeScope(Scope.Application);

            var groupManager = m_ApplicationScope.Resolve<IGroupManager>();
            groupManager.AddGroup("Test", m_DatabaseUri.ToString());

            m_Group = groupManager.OpenExclusively("Test");

            m_Synchronizer = m_Group.GetSynchronizer();
        }


        public override void Dispose()
        {
            m_Group.Dispose();
            m_ApplicationScope.Dispose();
            m_Container.Dispose();
            m_TempDirectory.Dispose();
            base.Dispose();
        }


        [Fact]
        public void Running_Synchronize_without_sync_folders_has_no_effect()
        {
            m_Synchronizer.Run();

            // check that no actions / conflicts have been stored
            var syncStateService = m_Group.GetSyncStateService();

            Assert.Null(syncStateService.LastSyncSnapshotId);
            Assert.Empty(syncStateService.Actions);
            Assert.Empty(syncStateService.Conflicts);
        }

        [Fact]
        public void Running_Synchronize_has_no_effect_if_there_is_a_sync_folder_without_history()
        {
            var historyBuilder1 = new HistoryBuilder(m_Group, "folder1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "folder2");

            m_Synchronizer.Run();

            // check that no actions or conflicts  have been stored
            var syncStateService = m_Group.GetSyncStateService();

            Assert.Null(syncStateService.LastSyncSnapshotId);
            Assert.Empty(syncStateService.Actions);
            Assert.Empty(syncStateService.Conflicts);
        }

        [Fact]
        public void Running_Synchronize_without_two_sync_folders_has_no_effect()
        {
            // ARRANGE
            var state = new Directory(null, "root")
            {
                d => new EmptyFile(d, "file1")
            };

            var configurationService = m_Group.GetConfigurationService();
            configurationService.AddItem(new SyncFolder("history1"));

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("history1");
            historyService["history1"].CreateSnapshot(state);

            // ACT
            m_Synchronizer.Run();


            // ASSERT
            var syncStateService = m_Group.GetSyncStateService();

            Assert.Null(syncStateService.LastSyncSnapshotId);
            Assert.Empty(syncStateService.Actions);
            Assert.Empty(syncStateService.Conflicts);
        }

        [Fact]
        public void Running_Synchronize_without_at_least_one_snapshot_per_history_has_no_effect()
        {
            //ARRANGE

            var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "history2");

            //ACT
            m_Synchronizer.Run();

            //ASSERT
            var syncStateService = m_Group.GetSyncStateService();

            Assert.Null(syncStateService.LastSyncSnapshotId);
            Assert.Empty(syncStateService.Actions);
            Assert.Empty(syncStateService.Conflicts);

        }

        [Fact]
        public void Running_Sychronize_without_any_changes_since_the_last_sync_point_produces_no_actions_or_conflicts()
        {
            //ARRANGE
            var state1 = new Directory(null, "root") { d => new EmptyFile(d, "file1") };
            var state2 = new Directory(null, "root") { d => new EmptyFile(d, "file2") };

            var configService = m_Group.GetConfigurationService();
            configService.AddItem(new SyncFolder("folder1") { Path = "Irrelevant" });
            configService.AddItem(new SyncFolder("folder2") { Path = "Irrelevant" });

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("folder1");
            historyService.CreateHistory("folder2");
            var snapshot1 = historyService["folder1"].CreateSnapshot(state1);
            var snapshot2 = historyService["folder2"].CreateSnapshot(state2);


            var syncStateService = m_Group.GetSyncStateService();
            var multiFileSystemHistoryService = m_Group.GetMultiFileSystemHistoryService();
            using (var updater = syncStateService.BeginUpdate(multiFileSystemHistoryService.CreateSnapshot().Id))
            {
                Assert.True(updater.TryApply());
            }

            //ACT
            m_Synchronizer.Run();

            //ASSERT            
            Assert.Empty(syncStateService.Actions);
            Assert.Empty(syncStateService.Conflicts);
        }

        //[Fact]
        //public void Synchronize_without_previous_sync_single_snapshots_and_no_conflicts()
        //{
        //    //ARRANGE
        //    var left = new HistoryBuilder(m_Group, "left");
        //    var right = new HistoryBuilder(m_Group, "right");

        //    left.AddFile("file1");
        //    right.AddFile("file2");

        //    var snapshot1 = left.CreateSnapshot();
        //    var snapshot2 = right.CreateSnapshot();

        //    //ACT
        //    m_Synchronizer.Run();


        //    //ASSERT
        //    var syncActionService = m_Group.GetSyncActionService();
        //    Assert.Equal(2, syncActionService.AllItems.Count());

        //    SyncAssert.ActionsExist(syncActionService, "/file1",
        //        expectedCount: 1,
        //        expectedState: SyncActionState.Queued,
        //        expectedChangeType: ChangeType.Added
        //        );

        //    SyncAssert.ToVersionMatches(left.CurrentState.GetFile("/file1"), syncActionService["/file1"].Single());

        //    SyncAssert.ActionsExist(syncActionService, "/file2",
        //        expectedCount: 1,
        //        expectedState: SyncActionState.Queued,
        //        expectedChangeType: ChangeType.Added);

        //    SyncAssert.ToVersionMatches(right.CurrentState.GetFile("/file2"), syncActionService["/file2"].Single());

        //    Assert.Empty(m_Group.GetSyncConflictService().Items);
        //    Assert.Single(m_Group.GetSyncPointService().Items);

        //    var syncPoint = m_Group.GetSyncPointService().Items.Single();
        //    Assert.Equal(1, syncPoint.Id);
        //    var expectedToSnapshotId = m_MultiFileSystemHistory.LatestSnapshot.Id;
        //    Assert.Equal(expectedToSnapshotId, syncPoint.MultiFileSystemSnapshotId);
        //}

        [Fact]
        public void Synchronize_without_previous_sync_no_conflicts()
        {
            // ARRANGE
            var left = new HistoryBuilder(m_Group, "left");
            left.AddFile("file1");
            left.CreateSnapshot();

            left.RemoveFile("file1");
            left.CreateSnapshot();

            var right = new HistoryBuilder(m_Group, "right");

            right.AddFile("file2");
            right.CreateSnapshot();

            right.RemoveFile("file2");
            right.CreateSnapshot();

            //ACT
            m_Synchronizer.Run();

            //ASSERT
            var syncStateService = m_Group.GetSyncStateService();
            var multiFileSystemHistoryService = m_Group.GetMultiFileSystemHistoryService();

            Assert.Empty(syncStateService.Conflicts);
            Assert.Empty(syncStateService.Actions);

            Assert.Equal(multiFileSystemHistoryService.LatestSnapshot.Id, syncStateService.LastSyncSnapshotId);
        }

        [Fact]
        public void Synchronize_Cycle_with_conflict()
        {
            //ARRANGE
            // changes to file1 (captial letters indicate file versions)
            // Left: A -> B -> C -> A
            // Right: B -> D

            var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(3) } };
            var d = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(4) } };

            var configurationService = m_Group.GetConfigurationService();
            configurationService.AddItem(new SyncFolder("left"));
            configurationService.AddItem(new SyncFolder("right"));

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["left"].CreateSnapshot(b);
            historyService["left"].CreateSnapshot(c);
            historyService["left"].CreateSnapshot(a);

            historyService["right"].CreateSnapshot(b);
            historyService["right"].CreateSnapshot(d);

            //ACT
            m_Synchronizer.Run();

            // ASSERT
            var syncStateService = m_Group.GetSyncStateService();

            //TODO: Check that the right snapshot Ids were saved for the conflict
            // Expected result: conflict between file versions A and D, no sync actions            
            Assert.Single(syncStateService.Conflicts);
            Assert.Empty(syncStateService.Actions);

        }

        [Fact]
        public void Synchronize_Cycle_without_conflict()
        {
            //ARRANGE
            // changes to file (captial letters indicate file versions)
            // Left: A -> B -> A
            // Right: C -> A

            var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(3) } };

            var configurationService = m_Group.GetConfigurationService();
            configurationService.AddItem(new SyncFolder("left"));
            configurationService.AddItem(new SyncFolder("right"));

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["left"].CreateSnapshot(b);
            historyService["left"].CreateSnapshot(a);

            historyService["right"].CreateSnapshot(c);
            historyService["right"].CreateSnapshot(a);


            //ACT
            m_Synchronizer.Run();

            var syncStateService = m_Group.GetSyncStateService();

            // ASSERT                        
            Assert.Empty(syncStateService.Conflicts);
            Assert.Empty(syncStateService.Actions);            
        }

        [Fact]
        public void Synchronize_with_previous_sync_point_01()
        {
            // ARRANGE      
            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            // left: add file 1
            // right: empty
            left.AddFile("file1");
            left.CreateSnapshot();
            right.AddFiles();
            right.CreateSnapshot();

            m_Synchronizer.Run();
            var syncStateService = m_Group.GetSyncStateService();

            Assert.Single(syncStateService.Actions);

            // left: add file 2
            left.AddFiles("file2");
            left.CreateSnapshot();

            //ACT
            m_Synchronizer.Run();

            Assert.Equal(2, syncStateService.Actions.Count);
            //TODO: Check contents of sync actions
        }
       

        //[Fact]
        //public void Synchronize_non_applicable_sync_actions_yields_conflict_and_get_cancelled()
        //{
        //    //ARRANGE
        //    // Left: A -> Sync ------> Sync -> Conflict
        //    // Right:     Sync -> B -> Sync -> Conflict
        //    var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
        //    var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(1) } };
        //    var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(2) } };

        //    var configurationService = m_Group.GetConfigurationService();
        //    configurationService.AddItem(new SyncFolder("left"));
        //    configurationService.AddItem(new SyncFolder("right"));

        //    var historyService = m_Group.GetHistoryService();
        //    historyService.CreateHistory("left");
        //    historyService.CreateHistory("right");

        //    historyService["left"].CreateSnapshot(a);
        //    historyService["right"].CreateSnapshot(new Directory(null, "root"));

        //    // ACT

        //    // first sync
        //    m_Synchronizer.Run();

        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
        //        expectedState: SyncActionState.Queued,
        //        expectedCount: 1,
        //        expectedChangeType: ChangeType.Added);
        //    Assert.Empty(m_Group.GetSyncConflictService().Items);

        //    historyService["right"].CreateSnapshot(b);

        //    // second sync

        //    m_Synchronizer.Run();

        //    // ASSERT

        //    // the sync action needs to be cancelled, a conflcit should exist
        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
        //        expectedState: SyncActionState.Cancelled,
        //        expectedCount: 1,
        //        expectedChangeType: ChangeType.Added);

        //    Assert.Single(m_Group.GetSyncConflictService().Items);
        //    var conflict = m_Group.GetSyncConflictService().Items.Single();
        //    Assert.Equal("/file", conflict.FilePath);
        //    Assert.Null(conflict.SnapshotId);
        //}


        //[Fact(Skip = "Not implemented")]
        //public void Synchronize_pending_actions_are_cancelled_if_a_conflict_is_detected()
        //{
        //    //ARRANGE
        //    // 1: A -> Sync -> B -> Sync
        //    // 2: A -> Sync -> C -> Sync
        //    // 3: * -> Sync -> ---> Sync
        //    var lastWriteTime = SystemClock.Instance.GetCurrentInstant();
        //    var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(1) } };
        //    var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(2) } };
        //    var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime + Duration.FromHours(3) } };

        //    var configurationService = m_Group.GetConfigurationService();
        //    configurationService.AddItem(new SyncFolder("1"));
        //    configurationService.AddItem(new SyncFolder("2"));
        //    configurationService.AddItem(new SyncFolder("3"));

        //    var historyService = m_Group.GetHistoryService();
        //    historyService.CreateHistory("1");
        //    historyService.CreateHistory("2");
        //    historyService.CreateHistory("3");

        //    historyService["1"].CreateSnapshot(a);
        //    historyService["2"].CreateSnapshot(a);
        //    historyService["3"].CreateSnapshot(new Directory(null, "root"));

        //    // ACT

        //    // first sync
        //    m_Synchronizer.Run();

        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
        //        expectedState: SyncActionState.Queued,
        //        expectedCount: 1,
        //        expectedChangeType: ChangeType.Added);
        //    Assert.Empty(m_Group.GetSyncConflictService().Items);


        //    historyService["1"].CreateSnapshot(b);
        //    historyService["2"].CreateSnapshot(c);


        //    // second sync

        //    m_Synchronizer.Run();

        //    // ASSERT

        //    // the sync action needs to be cancelled, a conflcit should exist
        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
        //        expectedState: SyncActionState.Cancelled,
        //        expectedCount: 1,
        //        expectedChangeType: ChangeType.Added);

        //    Assert.Single(m_Group.GetSyncConflictService().Items);
        //    var conflict = m_Group.GetSyncConflictService().Items.Single();
        //    Assert.Equal("/file", conflict.FilePath);
        //    Assert.Null(conflict.SnapshotId);
        //}


        //[Fact(Skip = "Not implemented")]
        //public void Synchronize_Unrelated_sync_actions_stay_unchanged()
        //{
        //    // ARRANGE                  
        //    var left = new HistoryBuilder(m_Group, "left");
        //    var right = new HistoryBuilder(m_Group, "right");

        //    left.AddFile("file1");

        //    left.CreateSnapshot();
        //    right.CreateSnapshot();


        //    //ACT

        //    // first sync
        //    m_Synchronizer.Run();

        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file1",
        //            expectedState: SyncActionState.Queued,
        //            expectedCount: 1,
        //            expectedChangeType: ChangeType.Added);

        //    Assert.Empty(m_Group.GetSyncConflictService().Items);

        //    left.AddFile("file2");
        //    left.CreateSnapshot();

        //    m_Synchronizer.Run();


        //    //ASSERT
        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file1",
        //            expectedState: SyncActionState.Queued,
        //            expectedCount: 1,
        //            expectedChangeType: ChangeType.Added);

        //    SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file2",
        //            expectedState: SyncActionState.Queued,
        //            expectedCount: 1,
        //            expectedChangeType: ChangeType.Added);

        //    Assert.Empty(m_Group.GetSyncConflictService().Items);
        //}

        //TODO: More tests

        //TODO: unrelated sync actions stay unchanged

        //TODO: Synchronize has no effect, if no new snapshots were added since the last sync

        //[Fact(Skip = "Not implemented")]
        //public void Sychronize_resets_the_sync_state_when_a_new_folder_was_added()
        //{
        //    // ARRANGE
        //    var left = new HistoryBuilder(m_Group, "left");
        //    var right = new HistoryBuilder(m_Group, "right");

        //    left.AddFile("file1");

        //    left.CreateSnapshot();
        //    right.CreateSnapshot();

        //    m_Synchronizer.Run();

        //    var firstSyncActions = m_Group.GetSyncActionService().AllItems.ToArray();

        //    // ACT: Add a new sync folder

        //    var newHistory = new HistoryBuilder(m_Group, "history3");
        //    newHistory.CreateSnapshot();

        //    m_Group.GetSyncConflictService().AddItems(new ConflictInfo("/file2", null));

        //    m_Synchronizer.Run();


        //    //ASSERT

        //    var syncPointService = m_Group.GetSyncPointService();

        //    // there should be 3 sync points (2 "regular" syncpoints and a "reset point")
        //    Assert.Equal(3, syncPointService.Items.Count());

        //    // first sync
        //    Assert.NotNull(syncPointService[1].MultiFileSystemSnapshotId);

        //    // reset
        //    Assert.Null(syncPointService[2].MultiFileSystemSnapshotId);

        //    // second sync (FromSnapshots needs to be reset to null)
        //    Assert.NotNull(syncPointService[3].MultiFileSystemSnapshotId);

        //    // all sync actions from previous syncs need to be cancelled
        //    var syncActions = m_Group.GetSyncActionService().AllItems.ToDictionary(a => a.Id);
        //    Assert.True(firstSyncActions.All(a => syncActions[a.Id].State == SyncActionState.Cancelled));

        //    // all conflicts need to be removed            
        //    Assert.Empty(m_Group.GetSyncConflictService().Items);
        //}


        //TODO: Reset occurs when a folder is removed


        //[Fact(Skip = "Not implemented")]
        //public void Synchronizer_creates_conflict_if_a_file_with_pending_syncactions_is_modified()
        //{
        //    // SCENARIO
        //    // left: A -> B *Sync* -> C  *Sync*
        //    // right A      *Sync*       *Sync*
        //    // 3: A

        //    // ARRANGE
        //    var writeTimeA = SystemClock.Instance.GetCurrentInstant();
        //    var writeTimeB = writeTimeA + Duration.FromHours(1);
        //    var writeTimeC = writeTimeA + Duration.FromHours(2);

        //    var historyBuilder1 = new HistoryBuilder(m_Group, "history1");
        //    historyBuilder1.AddFile("file1", writeTimeA);
        //    historyBuilder1.CreateSnapshot();
        //    historyBuilder1.RemoveFile("file1");
        //    historyBuilder1.AddFile("file1", writeTimeB);
        //    historyBuilder1.CreateSnapshot();

        //    var historyBuilder2 = new HistoryBuilder(m_Group, "history2");
        //    historyBuilder2.AddFile("file1", writeTimeA);
        //    historyBuilder2.CreateSnapshot();

        //    var historyBuilder3 = new HistoryBuilder(m_Group, "history3");
        //    historyBuilder3.AddFile("file1", writeTimeA);
        //    historyBuilder3.CreateSnapshot();

        //    m_Synchronizer.Run();

        //    historyBuilder1.RemoveFile("file1");
        //    historyBuilder1.AddFile("file1", writeTimeC);
        //    historyBuilder1.CreateSnapshot();

        //    // ACT
        //    m_Synchronizer.Run();

        //    // ASSERT: The pending sync action cannot be applied => create a conflict for the file

        //    var conflicts = m_Group.GetSyncConflictService().Items.ToArray();
        //    Assert.Single(conflicts);

        //    // snapshot ids must be null (there were no previous sync at the time the non-applicable sync action was created)
        //    Assert.Null(conflicts.Single().SnapshotId);

        //    Assert.Empty(m_Group.GetSyncActionService().PendingItems);

        //}

    }
}
