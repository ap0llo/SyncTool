using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SyncTool.Common
{
    public class SingleDirectoryGroupDirectoryPathProvider : IGroupDirectoryPathProvider
    {
        readonly string m_Directory;
       
        

        public SingleDirectoryGroupDirectoryPathProvider(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException(nameof(directory));
            }

            if (!System.IO.Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"The directory '{directory}' does not exist");
            }
            
            m_Directory = directory;
        }



        public string GetGroupDirectoryPath(string repositoryName) => Path.Combine(m_Directory, repositoryName);
       
        

    }
}