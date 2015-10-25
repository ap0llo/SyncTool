using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    /// <summary>    
    /// Converter that takes a "meta file system" and retrieves the original file system stored in there 
    /// </summary>
    internal class MetaFileSystemToFileSystemConverter
    {
        public IDirectory Convert(IDirectory directory)
        {
            var stacks = new Tuple<Stack<IDirectory>, Stack<IFile>>(new Stack<IDirectory>(), new Stack<IFile>());

            VisitDynamic(directory, stacks);
            return stacks.Item1.Pop();
        }


        void Visit(IDirectory directory, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            // visit directories and files

            foreach (var subDir in directory.Directories)
            {
                VisitDynamic(subDir, stacks);
            }

            foreach (var file in directory.Files)
            {
                VisitDynamic(file, stacks);
            }

            // create a new directory to replace the current one
            var newDir = new Directory(directory.Name);

            // remove subdirectories from the stack
            foreach (var _ in directory.Directories)
            {
                newDir.Add(stacks.Item1.Pop());
            }

            // get all files from the stack
            while (stacks.Item2.Count > 0)
            {
                newDir.Add(stacks.Item2.Pop());
            }

            // push new directory to the stack
            stacks.Item1.Push(newDir);
        }

        void Visit(FilePropertiesFile file, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            // load file properties
            var describedFile = new File(file.Content.Name)
            {
                LastWriteTime = file.Content.LastWriteTime,
                Length = file.Content.Length
            };

            stacks.Item2.Push(describedFile);
        }

        void Visit(IFile file, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            // ignore file instances that are not instances of FilePropertiesFile
        }

        void VisitDynamic(dynamic arg, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            ((dynamic) this).Visit(arg, stacks);
        }

    }
}