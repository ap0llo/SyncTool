using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    public sealed class MultiFileSystemChangeList 
    {
        readonly IDictionary<string, IEnumerable<Change>> m_Changes;


        public string Path { get; }

        public IEnumerable<string> HistoryNames => m_Changes.Keys;

        public IEnumerable<Change> AllChanges => HistoryNames.SelectMany(GetChanges).Distinct();


        public MultiFileSystemChangeList(string path, IEnumerable<string> historyNames)
        {
            PathValidator.EnsureIsValidFilePath(path);
            PathValidator.EnsureIsRootedPath(path);

            Path = path;
            m_Changes = historyNames.ToDictionary(
                keySelector: name => name, 
                elementSelector: name => Enumerable.Empty<Change>(),
                comparer: StringComparer.InvariantCultureIgnoreCase
            );
        }

        
        public IEnumerable<Change> GetChanges(string historyName) => m_Changes[historyName];

        internal void SetChanges(string historyName, IEnumerable<Change> changes)
        {
            m_Changes[historyName] = changes.ToArray();
        }
    }
}