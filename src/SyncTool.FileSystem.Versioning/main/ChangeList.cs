using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Versioning
{
    /// <summary>
    /// Groups all changes for a file
    /// </summary>
    public sealed class ChangeList
    {
        public string Path { get; }

        public IEnumerable<Change> Changes { get; }


        public ChangeList(IEnumerable<Change> changes)
        {
            changes = changes?.ToList() ?? throw new ArgumentNullException(nameof(changes));

            if (!changes.Any())
                throw new ArgumentException("ChangeList must not be empty", nameof(changes));

            var pathCount = changes.Select(c => c.Path).Distinct().Count();

            if (pathCount != 1)
                throw new ArgumentException("The changes in the change list must not refer to different file paths");

            Path = changes.First().Path;
            Changes = changes;
        }
    }
}