using System;

namespace SyncTool.Common.Groups
{
    /// <summary>
    /// Indicates that a group could not be added because a group with the specified groupName already exists
    /// </summary>
    [Serializable]
    public sealed class DuplicateGroupException : GroupManagerException
    {        
        private DuplicateGroupException(string message) : base(message)
        {            
        }


        public static DuplicateGroupException FromName(string groupName)
        {
            return new DuplicateGroupException($"A Group named '{groupName}' already exists");
        }

        public static DuplicateGroupException FromAddress(string address)
        {
            return new DuplicateGroupException($"A Group with '{address}' already exists");
        }
    }
}