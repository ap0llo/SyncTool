using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.FileSystem
{
    public class LocalFileSystemLoader
    {

        readonly string m_RootPath;

        public LocalFileSystemLoader(string rootPath)
        {
            m_RootPath = rootPath;
        }

        public Directory LoadFileSystem()
        {            
            var dirInfo = new DirectoryInfo(m_RootPath);
            return LoadDirectory(dirInfo);
        }
        

        Directory LoadDirectory(DirectoryInfo directoryInfo)
        {
            var directory = directoryInfo.ToDirectory();

            foreach (var file in directoryInfo.EnumerateFiles())
            {
                directory.Add(file.ToFile());
            }

            foreach (var dir in directoryInfo.EnumerateDirectories())
            {
                directory.Add(LoadDirectory(dir));
            }

            return directory;
        }
        

    }
}
