using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SyncTool.Synchronization.SyncActions;
using SyncTool.Synchronization.Transfer;

namespace SyncTool.TestHelpers
{
    public static class SynchronizationStateMockingHelper
    {

        public static Mock<ISynchronizationState> GetSynchronizationStateMock()
        {
            return new Mock<ISynchronizationState>(MockBehavior.Strict);
        }

        public static Mock<ISynchronizationState> WithEmptyActionLists(this Mock<ISynchronizationState> mock)
        {
            mock.Setup(m => m.QueuedActions).Returns(Enumerable.Empty<SyncAction>());
            mock.Setup(m => m.InProgressActions).Returns(Enumerable.Empty<SyncAction>());
            mock.Setup(m => m.CompletedActions).Returns(Enumerable.Empty<SyncAction>());
            return mock;
        }

        public static Mock<ISynchronizationState> WithQueuedActions(this Mock<ISynchronizationState> mock, params SyncAction[] actions)
        {
            mock.Setup(m => m.QueuedActions).Returns(actions);
            return mock;
        }

        public static Mock<ISynchronizationState> WithCompletedActions(this Mock<ISynchronizationState> mock, params SyncAction[] actions)
        {
            mock.Setup(m => m.CompletedActions).Returns(actions);
            return mock;
        }
        public static Mock<ISynchronizationState> WithInProgressActions(this Mock<ISynchronizationState> mock, params SyncAction[] actions)
        {
            mock.Setup(m => m.InProgressActions).Returns(actions);
            return mock;
        }


        public static Mock<ISynchronizationState> WithIds(this Mock<ISynchronizationState> mock, string globalId = "", string localId = "")
        {
            mock.Setup(m => m.GlobalSnapshotId).Returns(globalId);
            mock.Setup(m => m.LocalSnapshotId).Returns(localId);
            return mock;
        }

    }
}
