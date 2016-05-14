// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SyncTool.Common;
using SyncTool.Configuration;
using SyncTool.Configuration.Model;
using SyncTool.FileSystem;
using SyncTool.FileSystem.Versioning;
using SyncTool.Git.TestHelpers;
using SyncTool.Synchronization;
using SyncTool.Synchronization.Conflicts;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;
using SyncTool.TestHelpers;
using Xunit;

namespace SyncTool.Git.Synchronization
{
    /// <summary>
    /// Tests for <see cref="Synchronizer"/>.
    /// Tests are located in SyncTool.Git so git implementations of the services can be used instead of mocking everything
    /// </summary>
    public class SynchronizerTest : GitGroupBasedTest
    {
        readonly Synchronizer m_Instance;
        readonly IGroup m_Group;

        public SynchronizerTest()
        {
            m_Instance = new Synchronizer(EqualityComparer<IFileReference>.Default, new ChangeFilterFactory());
            m_Group = CreateGroup();
        }

        [Fact]
        public void Running_Synchronize_without_sync_folders_has_no_effect()
        {            
            m_Instance.Synchronize(m_Group);

            // check that no actions / conflicts or sync points have been stored

            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);

        }

        [Fact]
        public void Running_Synchronize_has_no_effect_if_there_is_a_sync_folder_without_history()
        {
            var historyBuilder1 = new HistoryBuilder(m_Group, "folder1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "folder2");

            m_Instance.Synchronize(m_Group);

            // check that no actions / conflicts or sync points have been stored

            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);
        }

        [Fact]
        public void Running_Synchronize_without_at_two_sync_folders_has_no_effect()
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
            m_Instance.Synchronize(m_Group);


            // ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);
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
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Empty(m_Group.GetSyncPointService().Items);
            
        }

        [Fact]
        public void Running_Sychronize_without_any_changes_since_the_last_sync_point_produces_no_actions_or_conflicts()
        {
            //ARRANGE
            var state1 = new Directory(null, "root") { d => new EmptyFile(d, "file1") };
            var state2 = new Directory(null, "root") { d => new EmptyFile(d, "file2")};

            var configService = m_Group.GetConfigurationService();
            configService.AddItem(new SyncFolder("folder1") { Path = "Irrelevant"});
            configService.AddItem(new SyncFolder("folder2") { Path = "Irrelevant"});

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("folder1");
            historyService.CreateHistory("folder2");
            var snapshot1 = historyService["folder1"].CreateSnapshot(state1);
            var snapshot2 = historyService["folder2"].CreateSnapshot(state2);

            // save a sync point
            var syncPoint = new MutableSyncPoint()
            {
                Id = 1,
                FromSnapshots = null,
                ToSnapshots = new Dictionary<string, string>()
                {
                    {"folder1", snapshot1.Id},
                    {"folder2", snapshot2.Id}
                },
                FilterConfigurations = new Dictionary<string, FilterConfiguration>()
                {
                    {"folder1", FilterConfiguration.Empty },
                    {"folder2", FilterConfiguration.Empty }
                }
            };

            m_Group.GetSyncPointService().AddItem(syncPoint);

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Equal(2, m_Group.GetSyncPointService().Items.Count());
        }
 
        [Fact]
        public void Synchronize_without_previous_sync_single_snapshots_and_no_conflicts()
        {
            //ARRANGE
            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            left.AddFile("file1");
            right.AddFile("file2");
                     
            var snapshot1 = left.CreateSnapshot();
            var snapshot2 = right.CreateSnapshot();

            //ACT
            m_Instance.Synchronize(m_Group);


            //ASSERT
            var syncActionService = m_Group.GetSyncActionService();
            Assert.Equal(2, syncActionService.AllItems.Count());            

            SyncAssert.ActionsExist(syncActionService, "/file1",
                expectedCount:1,
                expectedState: SyncActionState.Queued,
                expectedChangeType: ChangeType.Added
                );

            SyncAssert.ToVersionMatches(left.CurrentState.GetFile("/file1"), syncActionService["/file1"].Single());            

            SyncAssert.ActionsExist(syncActionService, "/file2",
                expectedCount:1,
                expectedState: SyncActionState.Queued,
                expectedChangeType: ChangeType.Added);

            SyncAssert.ToVersionMatches(right.CurrentState.GetFile("/file2"), syncActionService["/file2"].Single());
            
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            Assert.Single(m_Group.GetSyncPointService().Items);

            var syncPoint = m_Group.GetSyncPointService().Items.Single();
            Assert.Equal(1, syncPoint.Id);
            Assert.Null(syncPoint.FromSnapshots);
            var expectedToSnapshots = new Dictionary<string, string>()
            {
                { "left", snapshot1.Id },
                { "right", snapshot2.Id }
            };
            DictionaryAssert.Equal(expectedToSnapshots, syncPoint.ToSnapshots);
        }

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
            m_Instance.Synchronize(m_Group);

            //ASSERT
            Assert.Empty(m_Group.GetSyncConflictService().Items);
            var syncActions = m_Group.GetSyncActionService().AllItems.ToArray();
            Assert.Empty(syncActions);
            Assert.Single(m_Group.GetSyncPointService().Items);
        }
        
        [Fact]
        public void Synchronize_Cycle_with_conflict()
        {
            //ARRANGE
            // changes to file1 (captial letters indicate file versions)
            // Left: A -> B -> C -> A
            // Right: B -> D

            var lastWriteTime = DateTime.Now;
            var a =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };
            var d =  new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(4) } };

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
            m_Instance.Synchronize(m_Group);

            // ASSERT
            // Expected result: conflict between file versions A and D, no sync actions            
            Assert.Single(m_Group.GetSyncConflictService().Items);            
            //TODO: Check that the right snapshot Ids were saved for the conflict
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Single(m_Group.GetSyncPointService().Items);

        }

        [Fact]
        public void Synchronize_Cycle_without_conflict()
        {
            //ARRANGE
            // changes to file (captial letters indicate file versions)
            // Left: A -> B -> A
            // Right: C -> A

            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };

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
            m_Instance.Synchronize(m_Group);

            // ASSERT            
            var items = m_Group.GetSyncConflictService().Items.ToArray();
            Assert.Empty(items);            
            Assert.Empty(m_Group.GetSyncActionService().AllItems);
            Assert.Single(m_Group.GetSyncPointService().Items);

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

            m_Instance.Synchronize(m_Group);
            Assert.Equal(1, m_Group.GetSyncActionService().AllItems.Count());

            // left: add file 2
            left.AddFiles("file2");
            left.CreateSnapshot();         

            //ACT
            m_Instance.Synchronize(m_Group);

            Assert.Equal(2, m_Group.GetSyncPointService().Items.Count());
            Assert.Equal(2, m_Group.GetSyncActionService().AllItems.Count());
            //TODO: Check ids in syncpoints
        }


        [Fact]
        public void Synchronize_non_applicable_sync_actions_yields_conflict_and_get_cancelled()
        {
            //ARRANGE
            // Left: A -> Sync ------> Sync -> Conflict
            // Right:     Sync -> B -> Sync -> Conflict
            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };

            var configurationService = m_Group.GetConfigurationService();
            configurationService.AddItem(new SyncFolder("left" ));
            configurationService.AddItem(new SyncFolder("right"));

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("left");
            historyService.CreateHistory("right");

            historyService["left"].CreateSnapshot(a);
            historyService["right"].CreateSnapshot(new Directory(null, "root"));

            // ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file", 
                expectedState: SyncActionState.Queued, 
                expectedCount: 1,
                expectedChangeType: ChangeType.Added);
            Assert.Empty(m_Group.GetSyncConflictService().Items);

            historyService["right"].CreateSnapshot(b);

            // second sync
            
            m_Instance.Synchronize(m_Group);

            // ASSERT

            // the sync action needs to be cancelled, a conflcit should exist
            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Cancelled,
                expectedCount: 1,
                expectedChangeType: ChangeType.Added);

            Assert.Single(m_Group.GetSyncConflictService().Items);
            var conflict = m_Group.GetSyncConflictService().Items.Single();
            Assert.Equal("/file", conflict.FilePath);
            Assert.Null(conflict.SnapshotIds);
        }


        [Fact]
        public void Synchronize_pending_actions_are_cancelled_if_a_conflict_is_detected()
        {
            //ARRANGE
            // 1: A -> Sync -> B -> Sync
            // 2: A -> Sync -> C -> Sync
            // 3: * -> Sync -> ---> Sync
            var lastWriteTime = DateTime.Now;
            var a = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(1) } };
            var b = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(2) } };
            var c = new Directory(null, "root") { dir => new File(dir, "file") { LastWriteTime = lastWriteTime.AddHours(3) } };

            var configurationService = m_Group.GetConfigurationService();
            configurationService.AddItem(new SyncFolder("1"));
            configurationService.AddItem(new SyncFolder("2"));
            configurationService.AddItem(new SyncFolder("3"));

            var historyService = m_Group.GetHistoryService();
            historyService.CreateHistory("1");
            historyService.CreateHistory("2");
            historyService.CreateHistory("3");

           historyService["1"].CreateSnapshot(a);
           historyService["2"].CreateSnapshot(a);
           historyService["3"].CreateSnapshot(new Directory(null, "root"));

            // ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Queued,
                expectedCount: 1,
                expectedChangeType: ChangeType.Added);
            Assert.Empty(m_Group.GetSyncConflictService().Items);


            historyService["1"].CreateSnapshot(b);
            historyService["2"].CreateSnapshot(c);
            

            // second sync

            m_Instance.Synchronize(m_Group);

            // ASSERT

            // the sync action needs to be cancelled, a conflcit should exist
            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file",
                expectedState: SyncActionState.Cancelled,
                expectedCount: 1,
                expectedChangeType: ChangeType.Added);

            Assert.Single(m_Group.GetSyncConflictService().Items);
            var conflict = m_Group.GetSyncConflictService().Items.Single();
            Assert.Equal("/file", conflict.FilePath);            
            Assert.Null(conflict.SnapshotIds);
        }


        [Fact]
        public void Synchronize_Unrelated_sync_actions_stay_unchanged()
        {
            // ARRANGE                  
            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            left.AddFile("file1");

            left.CreateSnapshot();
            right.CreateSnapshot();


            //ACT

            // first sync
            m_Instance.Synchronize(m_Group);

            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file1",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1,
                    expectedChangeType: ChangeType.Added);

            Assert.Empty(m_Group.GetSyncConflictService().Items);

            left.AddFile("file2");
            left.CreateSnapshot();

            m_Instance.Synchronize(m_Group);


            //ASSERT
            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file1",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1,
                    expectedChangeType: ChangeType.Added);

            SyncAssert.ActionsExist(m_Group.GetSyncActionService(), "/file2",
                    expectedState: SyncActionState.Queued,
                    expectedCount: 1,
                    expectedChangeType: ChangeType.Added);

            Assert.Empty(m_Group.GetSyncConflictService().Items);
        }

        //TODO: More tests

        //TODO: unrelated sync actions stay unchanged

        //TODO: Synchronize has no effect, if no new snapshots were added since the last sync

        [Fact]
        public void Sychronize_resets_the_sync_state_when_a_new_folder_was_added()
        {
            // ARRANGE
            var left = new HistoryBuilder(m_Group, "left");
            var right = new HistoryBuilder(m_Group, "right");

            left.AddFile("file1");

            left.CreateSnapshot();
            right.CreateSnapshot();

            m_Instance.Synchronize(m_Group);

            var firstSyncActions = m_Group.GetSyncActionService().AllItems.ToArray();

            // ACT: Add a new sync folder

            var newHistory = new HistoryBuilder(m_Group, "history3");
            newHistory.CreateSnapshot();

            m_Group.GetSyncConflictService().AddItems(new ConflictInfo("/file2", null));

            m_Instance.Synchronize(m_Group);


            //ASSERT

            var syncPointService = m_Group.GetSyncPointService();
            
            // there should be 2 sync points 
            Assert.Equal(2, syncPointService.Items.Count());

            // first sync
            Assert.Null(syncPointService[1].FromSnapshots);
            Assert.NotNull(syncPointService[1].ToSnapshots);
            
            // second sync (FromSnapshots needs to be reset to null)
            Assert.Null(syncPointService[2].FromSnapshots);
            Assert.NotNull(syncPointService[2].ToSnapshots);

            // all sync actions from previous syncs need to be cancelled
            var syncActions = m_Group.GetSyncActionService().AllItems.ToDictionary(a => a.Id);
            Assert.True(firstSyncActions.All(a => syncActions[a.Id].State == SyncActionState.Cancelled));

            // all conflicts need to be removed            
            Assert.Empty(m_Group.GetSyncConflictService().Items);
        }

        
        //TODO: Reset occurs when a folder is removed
        
        [Fact]
        public void Synchronize_stores_the_filter_configuration_in_the_sync_point()
        {
            //ARRANGE
            var historyBuilder1 = new HistoryBuilder(m_Group, "folder1");
            historyBuilder1.AddFile("file1");
            historyBuilder1.CreateSnapshot();

            var historyBuilder2 = new HistoryBuilder(m_Group, "folder2");
            historyBuilder2.AddFile("file2");
            historyBuilder2.CreateSnapshot();

            var configurationService = m_Group.GetConfigurationService();
            var folder1 = configurationService["folder1"];

            //"not isEmpty()" will always be true for file paths
            folder1.Filter = new FilterConfiguration(FilterType.MicroscopeQuery, "not isEmpty()");

            configurationService.UpdateItem(folder1);

            var folder2 = configurationService["folder2"];
            folder2.Filter = FilterConfiguration.Empty;
            configurationService.UpdateItem(folder2);

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            var syncPointService = m_Group.GetSyncPointService();
            var syncPoint = syncPointService.Items.Single();
            
            var expected = new Dictionary<string, FilterConfiguration>()
            {
                {"folder1", folder1.Filter},
                {"folder2", folder2.Filter}
            };
            DictionaryAssert.Equal(expected, syncPoint.FilterConfigurations);
        }

        [Fact]
        public void Synchronizer_resets_the_sync_state_if_a_filter_was_modified_since_the_lasr_sync()
        {
            //ARRANGE
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder1");
                historyBuilder.AddFile("file1");
                historyBuilder.CreateSnapshot();
            }
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder2");
                historyBuilder.AddFile("file2");
                historyBuilder.CreateSnapshot();
            }

            m_Instance.Synchronize(m_Group);

            // change filter for folder2
            {
                var configurationService = m_Group.GetConfigurationService();
                var folder = configurationService["folder2"];

                //"not isEmpty()" will always be true for file paths
                folder.Filter = new FilterConfiguration(FilterType.MicroscopeQuery, "not isEmpty()");

                configurationService.UpdateItem(folder);
            }

            var firstSyncActions = m_Group.GetSyncActionService().AllItems.ToArray();

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT
            {
                //ASSERT

                var syncPointService = m_Group.GetSyncPointService();

                // there should be 2 sync points 
                Assert.Equal(2, syncPointService.Items.Count());

                // first sync
                Assert.Null(syncPointService[1].FromSnapshots);
                Assert.NotNull(syncPointService[1].ToSnapshots);

                // second sync (FromSnapshots needs to be reset to null)
                Assert.Null(syncPointService[2].FromSnapshots);
                Assert.NotNull(syncPointService[2].ToSnapshots);

                // all sync actions from previous syncs need to be cancelled
                var syncActions = m_Group.GetSyncActionService().AllItems.ToDictionary(a => a.Id);
                Assert.True(firstSyncActions.All(a => syncActions[a.Id].State == SyncActionState.Cancelled));

                // all conflicts need to be removed            
                Assert.Empty(m_Group.GetSyncConflictService().Items);
            }
        }

        //TODO: Reset occurs when a filter is modified

        [Fact]
        public void Synchronize_ignores_changes_from_a_folder_excluded_by_the_folders_filter()
        {
            //ARRANGE

            var configurationService = m_Group.GetConfigurationService();

            // set up folder 1
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder1");
                historyBuilder.AddFile("file.excluded");
                historyBuilder.AddFile("file.included");
                historyBuilder.CreateSnapshot();

                // set up filter
                historyBuilder.SyncFolder.Filter = new FilterConfiguration(FilterType.MicroscopeQuery, "not endsWith('excluded')");
                configurationService.UpdateItem(historyBuilder.SyncFolder);
            }

            // set up folder 2 
            {
                new HistoryBuilder(m_Group, "folder2").CreateSnapshot();
            }
            // set up folder 3
            {
                new HistoryBuilder(m_Group, "folder3").CreateSnapshot();
            }


            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT: file.excluded is added to no folder (filtered from source, but file.included needs to be added to both folder 2 and 3)

            var syncActionService = m_Group.GetSyncActionService();
            var pendingItems = syncActionService.PendingItems.ToArray();
            
                    
            Assert.Equal(2, syncActionService.PendingItems.Count());

            SyncAssert.ActionsExist(syncActionService, 
                path: "/file.included",
                expectedState:SyncActionState.Queued,
                expectedCount:2,
                expectedChangeType: ChangeType.Added);
        }

        [Fact]
        public void Synchronize_does_not_apply_changes_to_a_folder_that_are_excluced_by_its_filter()
        {
            // ARRANGE

            var configurationService = m_Group.GetConfigurationService();

            // set up folder 1
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder1");
                historyBuilder.AddFile("file1");
                historyBuilder.AddFile("file2");
                historyBuilder.CreateSnapshot();
                
            }
            // set up folder 2
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder2");
                historyBuilder.CreateSnapshot();

                // set up filter
                historyBuilder.SyncFolder.Filter = new FilterConfiguration(FilterType.MicroscopeQuery, "endsWith('file1')");
                configurationService.UpdateItem(historyBuilder.SyncFolder);
            }

            // set up folder 3
            {
                var historyBuilder = new HistoryBuilder(m_Group, "folder3");
                historyBuilder.CreateSnapshot();
            }

            //ACT
            m_Instance.Synchronize(m_Group);

            //ASSERT: file1 is added to both folder2 and folder3, file2 is only added to folder3 (excluded by folder2's filter)
            var syncActionService = m_Group.GetSyncActionService();

            SyncAssert.ActionsExist(syncActionService, "/file1",
                expectedState: SyncActionState.Queued,
                expectedCount:2,
                expectedChangeType: ChangeType.Added);

            SyncAssert.ActionsExist(syncActionService, "/file2",
                expectedState: SyncActionState.Queued,
                expectedCount: 1,
                expectedChangeType: ChangeType.Added);
        }

        

        public override void Dispose()
        {
            m_Group.Dispose();
            base.Dispose();
        }





        class HistoryBuilder
        {
            readonly IGroup m_Group;
            readonly string m_Name;
            readonly IDictionary<string, File> m_Files = new Dictionary<string, File>(StringComparer.InvariantCultureIgnoreCase);


            public SyncFolder SyncFolder { get; }

            public IDirectory CurrentState { get; private set; }

            public HistoryBuilder(IGroup group, string name)
            {
                m_Group = @group;
                m_Name = name;
                m_Group.GetHistoryService().CreateHistory(m_Name);

                SyncFolder = new SyncFolder(m_Name) {Path = "Irrelevant"};

                m_Group.GetConfigurationService().AddItem(SyncFolder);
            }

            
            public void AddFile(string file) => AddFiles(file);

            public void AddFiles(params string[] files)
            {
                foreach (var fileName in files)
                {
                    if (!m_Files.ContainsKey(fileName))
                    {
                        m_Files.Add(fileName, new File(null, fileName) { LastWriteTime = DateTime.Now });
                    }
                }
                UpdateCurrentState();
            }

            public void RemoveFile(string file) => RemoveFiles(file);

            public void RemoveFiles(params string[] files)
            {
                foreach (var fileName in files)
                {
                    m_Files.Remove(fileName);
                }
                UpdateCurrentState();
            }
                   

            public IFileSystemSnapshot CreateSnapshot()
            {
                UpdateCurrentState();
                return m_Group.GetHistoryService()[m_Name].CreateSnapshot(CurrentState);
            }


            void UpdateCurrentState()
            {
                var dir = new Directory(null, "root");
                foreach (var file in m_Files.Values)
                {
                    dir.Add(d => file.WithParent(d));
                }
                CurrentState = dir;
            }
        }

    }
}