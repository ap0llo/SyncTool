using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SyncTool.Utilities
{
    public static class TemporaryDirectoryExtensions
    {

        public static IEnumerable<FileInfo> CreateFiles(this TemporaryDirectory temporaryDirectory, params string[] names)
        {
            return names.Select(name =>
            {
                var file = new FileInfo(Path.Combine(temporaryDirectory.Directory.FullName, name));

                using (file.Create()) { }
                
                return file;

            }).ToList();            
        }


        public static IEnumerable<DirectoryInfo> CreateDirectories(this TemporaryDirectory temporaryDirectory, params string[] names)
        {
            return names.Select(name =>
            {
                var dir = new DirectoryInfo(Path.Combine(temporaryDirectory.Directory.FullName, name));
                dir.Create();                
                return dir;

            }).ToList();
        } 


    }
}