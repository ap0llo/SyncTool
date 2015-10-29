using System;
using System.Collections.Generic;

namespace SyncTool.FileSystem.Git
{
    /// <summary>
    ///  Converter that replaces all file instances in a directory with instances of <see cref="FilePropertiesFile"/> when appropriate
    /// </summary>
    public class MetaFileSystemLoader : FileSystemConverter
    {
        public void Visit(IReadableFile file, Tuple<Stack<IDirectory>, Stack<IFile>> stacks)
        {
            // replace all files which's name ends with .SyncTool.json with a FilePropertiesFile
            if (file.Name.EndsWith(FilePropertiesFile.FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                stacks.Item2.Push(FilePropertiesFile.Load(file));
            }   
            else if (file.Name.Equals(DirectoryPropertiesFile.FileName, StringComparison.InvariantCultureIgnoreCase))
            {
                stacks.Item2.Push(DirectoryPropertiesFile.Load(file));
            }         
            else
            {
                // leave other files unchanged
                stacks.Item2.Push(file);
            }
        }
    }
}