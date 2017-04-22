using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    public class MultiFileSystemChangeList : IMultiFileSystemChangeList
    {
        readonly IDictionary<string, IEnumerable<IChange>> m_Changes;


        public string Path { get; }

        public IEnumerable<string> HistoryNames => m_Changes.Keys;

        public IEnumerable<IChange> AllChanges => HistoryNames.SelectMany(GetChanges).Distinct();


        public MultiFileSystemChangeList(string path, IEnumerable<string> historyNames)
        {
            PathValidator.EnsureIsValidFilePath(path);
            PathValidator.EnsureIsRootedPath(path);

            Path = path;
            m_Changes = historyNames.ToDictionary(
                keySelector: name => name, 
                elementSelector: name => Enumerable.Empty<IChange>(),
                comparer: StringComparer.InvariantCultureIgnoreCase
            );
        }

        
        public IEnumerable<IChange> GetChanges(string historyName) => m_Changes[historyName];

        public void SetChanges(string historyName, IEnumerable<IChange> changes)
        {
            m_Changes[historyName] = changes.ToArray();
        }

    }
}