using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    static class TypeMapper
    {
        static readonly IDictionary<Type, string> s_TableNames = new Dictionary<Type, string>
        {
            {typeof(DirectoryInstanceDo), "DirectoryInstances" },
            {typeof(DirectoryDo), "Directories" },
            {typeof(FileInstanceDo), "FileInstances" },
            {typeof(FileDo), "Files" }
        };

        public static string Table<T>() => s_TableNames[typeof(T)];

    }
}
