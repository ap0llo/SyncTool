using System.Collections.Generic;

namespace SyncTool.Sql.Model
{
    static class DirectoryInstanceDoExtensions
    {
        public static List<FileInstanceDo> GetFilesRecursively(this DirectoryInstanceDo directory)
        {
            var fileList = new List<FileInstanceDo>();
            DoGetFilesRecursively(directory, fileList);
            return fileList;
        }

        static void DoGetFilesRecursively(DirectoryInstanceDo directory, List<FileInstanceDo> fileList)
        {
            fileList.AddRange(directory.Files);
            foreach(var child in directory.Directories)
            {
                DoGetFilesRecursively(child, fileList);
            }
        }
    }
}
