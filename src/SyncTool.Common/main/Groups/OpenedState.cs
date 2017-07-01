using System;

namespace SyncTool.Common.Groups
{
    /// <summary>
    /// Encapsulates the state of a group (opened exclusively or shared)
    /// </summary>
    class OpenedState
    {
        //The state is encoded as integer
        // 0 => not opened
        // > 0 => the number of times the group is currently opened in shared mode
        // < 0 => the number of times the group is currently opened exclusively 
        int m_Value;


        // group can be open shared if it is not opened and not opend exclusively
        public bool CanOpenShared => m_Value >= 0;

        /// group can only be opened exclusively if it is not opened at all
        public bool CanOpenExclusively => m_Value == 0;


        public void NotifyOpenedShared()
        {
            // ensure group is currently not opened exclusively
            if (m_Value < 0)
                throw new InvalidOperationException();

            m_Value += 1;
        }

        public void NotifyClosedShared()
        {
            // ensure group is opened shared before decrementing value
            if (m_Value < 1)
                throw new InvalidOperationException();

            m_Value -= 1;
        }


        public void NotifyOpenedExclusively()
        {
            // ensure group is not opened
            if (m_Value != 0)
                throw new InvalidOperationException();

            m_Value = -1;
        }

        public void NotifyClosedExclusively()
        {
            // ensure group is opened exclusively before closing it
            if (m_Value != -1)
                throw new InvalidOperationException();

            m_Value = 0;
        }
    }
}