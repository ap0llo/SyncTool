// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    public class FileSystemConverter : BaseVisitor<Tuple<Stack<IDirectory>, Stack<IFile>>>
    {
        public IDirectory Convert(IDirectory directory)
        {
            var stacks = new Tuple<Stack<IDirectory>, Stack<IFile>>(new Stack<IDirectory>(), new Stack<IFile>());

            ((dynamic)this).Visit(directory, stacks);
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
                        
            while(stacks.Item2.Count > 0)
            {                
                newDir.Add(stacks.Item2.Pop());
            }

            stacks.Item1.Push(newDir);
        }


        public void Visit(IFile file, Tuple<Stack<IDirectory>, Stack<IFile>> parameter)
        {
            parameter.Item2.Push(file);
        }
    }
}