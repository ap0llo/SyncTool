using SyncTool.Sql.Model.Tables;
using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    static class DirectoryInstanceDoExtensions
    {
        public static List<FileInstancesTable.Record> GetFilesRecursively(this DirectoryInstancesTable.Record directory)
        {
            var fileList = new List<FileInstancesTable.Record>();
            DoGetFilesRecursively(directory, fileList);
            return fileList;
        }

        static void DoGetFilesRecursively(DirectoryInstancesTable.Record directory, List<FileInstancesTable.Record> fileList)
        {
            fileList.AddRange(directory.Files);
            foreach(var child in directory.Directories)
            {
                DoGetFilesRecursively(child, fileList);
            }
        }
    }
}
