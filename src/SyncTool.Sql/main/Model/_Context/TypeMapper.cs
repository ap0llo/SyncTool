using System;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    static class TypeMapper
    {
        static readonly IDictionary<Type, string> s_TableNames = new Dictionary<Type, string>
        {
            {typeof(DirectoryInstanceDo), "DirectoryInstances" },
            {typeof(DirectoryDo), "Directories" },
            {typeof(FileInstanceDo), "FileInstances" },
            {typeof(FileDo), "Files" },
            {typeof(FileSystemHistoryDo), "FileSystemHistories" },
            {typeof(SyncFolderDo), "SyncFolders" },
            {typeof(FileSystemSnapshotDo), "FileSystemSnapshots" }
        };

        public static string Table<T>() => s_TableNames[typeof(T)];

    }
}
