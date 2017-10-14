using System;

namespace SyncTool.FileSystem.Versioning
{
    public sealed class HistoryChange : IEquatable<HistoryChange>
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

        public override bool Equals(object obj) => Equals(obj as HistoryChange);

        public bool Equals(HistoryChange other)
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