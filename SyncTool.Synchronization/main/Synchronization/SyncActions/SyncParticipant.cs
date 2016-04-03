// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.Synchronization.SyncActions
{
    [Obsolete]
    public enum SyncParticipant
    {
        Left,
        Right
    }

    public static class SyncParticipantExtensions
    {
        public static SyncParticipant Invert(this SyncParticipant value)
        {
            switch (value)
            {
                case SyncParticipant.Left:
                    return SyncParticipant.Right;
                    
                case SyncParticipant.Right:
                    return SyncParticipant.Left;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
}