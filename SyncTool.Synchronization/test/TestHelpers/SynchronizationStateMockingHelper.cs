// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015-2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using SyncTool.Synchronization.SyncActions;
using SyncTool.Synchronization.State;

namespace SyncTool.TestHelpers
{
    public static class SynchronizationStateMockingHelper
    {

        public static Mock<ISynchronizationState> GetSynchronizationStateMock()
        {
            return new Mock<ISynchronizationState>(MockBehavior.Strict);
        }


        public static Mock<ISynchronizationState> WithId(this Mock<ISynchronizationState> mock, int id)
        {
            mock.Setup(m => m.Id).Returns(id);
            return mock;
        }



        public static Mock<ISynchronizationState> WithoutFromSnapshots(this Mock<ISynchronizationState> mock)
        {
            mock.Setup(m => m.FromSnapshots).Returns((IReadOnlyDictionary<string, string>)null);
            return mock;
        }

        public static Mock<ISynchronizationState> WithToSnapshot(this Mock<ISynchronizationState> mock, string name, string id)
        {
            IDictionary<string, string> current;
            try
            {
                current = (IDictionary<string, string>) mock.Object.ToSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? new Dictionary<string, string>();

            current.Add(name, id);
            mock.Setup(m => m.ToSnapshots).Returns((IReadOnlyDictionary<string, string>)current);
            
            return mock;
        }

        public static Mock<ISynchronizationState> WithFromSnapshot(this Mock<ISynchronizationState> mock, string name, string id)
        {
            IDictionary<string, string> current;
            try
            {
                current = (IDictionary<string, string>)mock.Object.FromSnapshots;
            }
            catch (Exception)
            {
                current = null;
            }
            current = current ?? new Dictionary<string, string>();

            current.Add(name, id);
            mock.Setup(m => m.FromSnapshots).Returns((IReadOnlyDictionary<string, string>)current);

            return mock;
        }


    }
}
