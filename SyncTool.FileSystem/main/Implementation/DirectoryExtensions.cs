using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem
{
    public static class DirectoryExtensions
    {


        public static IEnumerable<File> GetFlatFileList(this Directory directory)
        {
            var result = new List<File>();            
            GetFlatFileListHelper(directory, result);
            return result;
        }




        static void GetFlatFileListHelper(Directory directory, List<File> resultList)
        {
            

            resultList.AddRange(directory.Files);


            foreach (var dir in directory.Directories)
            {
                GetFlatFileListHelper(dir, resultList);
            }            
        }

    }
}