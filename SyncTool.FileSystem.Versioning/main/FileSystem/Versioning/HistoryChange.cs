// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------

using System;

namespace SyncTool.FileSystem.Versioning
{
    public class HistoryChange : IHistoryChange
    {        
        public string HistoryName { get; }

        public ChangeType Type { get; }


        public HistoryChange(string historyName, ChangeType type)
        {
            if(String.IsNullOrWhiteSpace(historyName))
                throw new ArgumentNullException(historyName);

            HistoryName = historyName;
            Type = type;
        }


        public override int GetHashCode() => StringComparer.InvariantCultureIgnoreCase.GetHashCode(HistoryName) | Type.GetHashCode();

        public override bool Equals(object obj) => Equals(obj as IHistoryChange);

        public bool Equals(IHistoryChange other)
        {
            if(other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return StringComparer.InvariantCultureIgnoreCase.Equals(HistoryName, other.HistoryName) &&
                   Type == other.Type;
        }
    }
}