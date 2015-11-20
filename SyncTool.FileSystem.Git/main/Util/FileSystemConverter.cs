// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2015, Andreas Grünwald
//  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// -----------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;

namespace SyncTool.FileSystem.Git
{
    public abstract class FileSystemConverter : BaseVisitor<Tuple<Stack<IDirectory>, Stack<IFile>>>
    {
        public IDirectory Convert(IDirectory directory)
        {
            return (IDirectory) ConvertDynamic(null, directory);
        }



        protected dynamic ConvertDynamic(IDirectory newParent, dynamic toConvert)
        {
            return ((dynamic)this).Convert(newParent, toConvert);
        }



        public abstract IDirectory Convert(IDirectory newParent, IDirectory toConvert);

        public abstract IFile Convert(IDirectory newParent, IFile toConvert);
        
    }
}