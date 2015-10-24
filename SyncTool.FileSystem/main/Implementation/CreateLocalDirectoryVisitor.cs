using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;


namespace SyncTool.FileSystem
{
    public class CreateLocalDirectoryVisitor
    {
        /// <summary>
        /// Creates the specified file system tree in the specified location
        /// </summary>
        /// <param name="toCreate">The file system tree to create</param>
        /// <param name="createIn">The toCreate to create the tree in</param>
        /// <returns></returns>
        public ILocalDirectory CreateDirectory(IDirectory toCreate, string createIn)
        {
            if (toCreate == null)
            {
                throw new ArgumentNullException(nameof(toCreate));
            }

            Visit((dynamic)toCreate, createIn);            
            return new LocalDirectory(Path.Combine(createIn, toCreate.Name));
        }

        public void Visit(IFile file, string parentPath)
        {
            using (System.IO.File.Create(Path.Combine(parentPath, file.Name)))
            {
            }
        }

        public void Visit(IReadableFile file, string parentPath)
        {
            using (var outputStream = System.IO.File.Create(Path.Combine(parentPath, file.Name)))
            {
                using (var inputStream = file.Open(FileMode.Open))
                {
                    inputStream.CopyTo(outputStream);
                }
            }
        }


        public void Visit(IDirectory directory, string parentPath)
        {
            var dirPath = Path.Combine(parentPath, directory.Name);

            var dirInfo = new DirectoryInfo(dirPath);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            foreach (var subDir in directory.Directories)
            {
                Visit((dynamic)subDir, dirPath);
            }

            foreach (var file in directory.Files)
            {
                Visit((dynamic) file, dirPath);
            }

        }
    }
}