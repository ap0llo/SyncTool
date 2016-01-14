// // -----------------------------------------------------------------------------------------------------------
// //  Copyright (c) 2015, Andreas Grünwald
// //  Licensed under the MIT License. See LICENSE.txt file in the project root for full license information.  
// // -----------------------------------------------------------------------------------------------------------

using System.IO;
using SyncTool.FileSystem;
using SyncTool.Synchronization.SyncActions;

namespace SyncTool.Git.FileSystem
{
    public class SyncActionFile : DataFile<SyncAction>
    {
        readonly SyncActionSerializer m_Serializer = new SyncActionSerializer();


        public const string FileSuffix = ".SyncAction.json";


        public SyncActionFile(IDirectory parent, SyncAction content) : base(parent, content.Id.ToString("D") + FileSuffix , content)
        {
        }

        private SyncActionFile(IDirectory parent, string name, SyncAction content) : base(parent, name, content)
        {
            
        }


        public override Stream OpenRead()
        {
            using (var memoryStream = new MemoryStream())
            {
                m_Serializer.Serialize(this.Content, memoryStream);
                return new MemoryStream(memoryStream.ToArray());
            }            
        }

        public override IFile WithParent(IDirectory newParent) => new SyncActionFile(newParent, Name, Content);
    }
}