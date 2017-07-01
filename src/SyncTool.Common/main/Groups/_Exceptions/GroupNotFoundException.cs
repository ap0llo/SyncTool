using System;
using System.Runtime.Serialization;

namespace SyncTool.Common.Groups
{
    [Serializable]
    public class GroupNotFoundException : GroupManagerException
    {
        public string GroupName { get; set; }

        public GroupNotFoundException(string groupName) : base($"The Group '{groupName}' could not be found")
        {
            GroupName = groupName;
        }

        protected GroupNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}