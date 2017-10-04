using System;
using System.Collections.Generic;
using System.Text;

namespace SyncTool.Synchronization.State
{
    public interface ISyncStateService
    {
        
        ISyncStateUpdater BeginUpdate();
    }
}
