using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    public class MetaFileSystemCreator : BaseVisitor<Tuple<Stack<IDirectory>, Stack<IFile>>>
    {
        

        public IDirectory CreateMetaDirectory(IDirectory directory)
        {
            var stacks = new Tuple<Stack<IDirectory>, Stack<IFile>>(new Stack<IDirectory>(), new Stack<IFile>());

            ((dynamic) this).Visit(directory, stacks);
            return stacks.Item1.Pop();
        }



        public override void Visit(IDirectory directory, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {          
            base.Visit(directory, stacks);

            var newDir = new Directory(directory.Name);

            foreach (var _ in directory.Directories)
            {
                newDir.Add(stacks.Item1.Pop());
            }

            foreach (var _ in directory.Files)
            {
                newDir.Add(stacks.Item2.Pop());
            }

            stacks.Item1.Push(newDir);
        }


        public override void Visit(IFile file, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            stacks.Item2.Push(new FilePropertiesFile(file));
        }
    }
}