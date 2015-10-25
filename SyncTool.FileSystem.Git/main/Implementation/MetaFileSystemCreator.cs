using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    public class MetaFileSystemCreator : FileSystemConverter
    {

        public IDirectory CreateMetaDirectory(IDirectory directory) => this.Convert(directory);
        

        public void Visit(IFile file, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            stacks.Item2.Push(FilePropertiesFile.ForFile(file));
        }

    }
}