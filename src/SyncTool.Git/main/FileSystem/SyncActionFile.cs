﻿// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015-2016, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.IO;
using SyncTool.FileSystem;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Git.FileSystem
{
    public class SyncActionFile : DataFile<SyncAction>
    {
        static readonly SyncActionSerializer s_Serializer = new SyncActionSerializer();


        public const string FileNameSuffix = ".SyncAction.json";


        public SyncActionFile(IDirectory parent, SyncAction content) : base(parent, GetFileName(content) , content)
        {
        }

        private SyncActionFile(IDirectory parent, string name, SyncAction content) : base(parent, name, content)
        {
            
        }


        public override Stream OpenRead()
        {
            using (var memoryStream = new MemoryStream())
            {
                s_Serializer.Serialize(this.Content, memoryStream);
                return new MemoryStream(memoryStream.ToArray());
            }            
        }

        public override IFile WithParent(IDirectory newParent) => new SyncActionFile(newParent, Name, Content);


        internal static string GetFileName(SyncAction action) => action.Id.ToString("D") + FileNameSuffix;

        public static SyncActionFile Load(IDirectory parent, IReadableFile file)
        {
            if (file == null)
            {
                throw new ArgumentNullException(nameof(file));
            }

            if (!file.Name.EndsWith(FileNameSuffix, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException($"File name Name must end with {FileNameSuffix}", nameof(file));
            }

            using (var stream = file.OpenRead())
            {
                return new SyncActionFile(parent, s_Serializer.Deserialize(stream));
            }
        }

    }
}