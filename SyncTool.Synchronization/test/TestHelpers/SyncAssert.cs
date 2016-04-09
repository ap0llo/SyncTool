// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using SyncTool.FileSystem;
using SyncTool.Synchronization.State;
using SyncTool.Synchronization.SyncActions;
using Xunit;

namespace SyncTool.TestHelpers
{
    public static class SyncAssert
    {


        public static void Equal(ISyncPoint expected, ISyncPoint actual)
        {
            Assert.Equal(expected.Id, actual.Id);   
            DictionaryAssert.Equal(expected.FromSnapshots, actual.FromSnapshots);
            DictionaryAssert.Equal(expected.ToSnapshots, actual.ToSnapshots);
        }


        public static void ActionsExist<T>(ISyncActionService service, string path, SyncActionState? expectedState = null, int? expectedCount = null)
        {
            ActionsExist(service, path, expectedState, expectedCount, typeof(T));
        }

        public static void ActionsExist(ISyncActionService service, string path, SyncActionState? expectedState = null, int? expectedCount = null, Type expectedType = null)
        {
            var actions = service[path].ToArray();

            Assert.NotEmpty(actions);

            if (expectedCount.HasValue)
            {
                Assert.Equal(expectedCount, actions.Length);
            }

            if (expectedState.HasValue)
            {                                   
                SyncAssert.IsInState(expectedState.Value, service["/file1"].ToArray());
            }

            if (expectedType != null)
            {
                SyncAssert.IsType(expectedType, actions);
            }

        }


        public static void IsInState(SyncActionState expectedState, params SyncAction[] actions)
        {
            foreach (var action in actions)
            {
                Assert.Equal(expectedState, action.State);
            }
        }

        public static void IsType(Type type, params SyncAction[] objects)
        {
            foreach (var obj in objects)
            {
                Assert.IsType(type, obj);
            }
        }


        public static void NewFileMacthes(IFile file, params SyncAction[] actions)
        {
            foreach (var action in actions.Cast<AddFileSyncAction>())
            {
                Assert.True(action.NewFile.Matches(file));
            }
        }

    }
}