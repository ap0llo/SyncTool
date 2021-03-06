﻿using System;
using System.IO;

namespace SyncTool.FileSystem.Local
{   
    public class LocalItemCreator : BaseVisitor<string>
    {

        /// <summary>
        /// Creates the specified file system tree in the specified location
        /// </summary>
        /// <param name="toCreate">The file system tree to create</param>
        /// <param name="createIn">The path to create the tree in</param>
        /// <returns></returns>
        public ILocalDirectory CreateDirectory(IDirectory toCreate, string createIn)
        {
            if (toCreate == null)
            {
                throw new ArgumentNullException(nameof(toCreate));
            }

            VisitDynamic(toCreate, createIn);
            return new LocalDirectory(null, Path.Combine(createIn, toCreate.Name));
        }

        /// <summary>
        /// Creates the specified file in the specified directory
        /// </summary>
        public ILocalFile CreateFile(IFile toCreate, string createIn)
        {
            if (toCreate == null)
            {
                throw new ArgumentNullException(nameof(toCreate));
            }

            VisitDynamic(toCreate, createIn);
            return new LocalFile(null, Path.Combine(createIn, toCreate.Name));
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
                using (var inputStream = file.OpenRead())
                {
                    inputStream.CopyTo(outputStream);
                }
            }
        }

        public override void Visit(IDirectory directory, string parentPath)
        {
            var dirPath = Path.Combine(parentPath, directory.Name);

            var dirInfo = new DirectoryInfo(dirPath);

            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            base.Visit(directory, dirPath);
        }


        void VisitDynamic(dynamic arg, string parentPath)
        {
            ((dynamic)this).Visit(arg, parentPath);
        }
    }
}