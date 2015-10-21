using System;
using System.IO;

namespace SyncTool.Utilities
{
    public class TemporaryDirectory : IDisposable
    {
        readonly DirectoryInfo m_Directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));



        public DirectoryInfo Directory => m_Directory;

        public TemporaryDirectory()
        {
            m_Directory.Create();            
        }


        public virtual void Dispose()
        {
            // remove read-only flag from all files before deleting    
            Directory.Attributes = FileAttributes.Normal;
            foreach (var info in m_Directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            m_Directory.Delete(true);
        }
    }
}