// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
namespace SyncTool.FileSystem
{
    public abstract class BaseVisitor<T>
    {
        public virtual void Visit(IDirectory directory, T parameter)
        {            
            foreach (var subDir in directory.Directories)
            {
                ((dynamic)this).Visit((dynamic)subDir, parameter);
            }

            foreach (var file in directory.Files)
            {
                ((dynamic)this).Visit((dynamic) file, parameter);
            }
        }
    }
}