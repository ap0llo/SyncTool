using System;

namespace SyncTool.Common
{
    public class GroupSettings
    {
        public string Name { get; }
        
        public string Address { get; }


        public GroupSettings(string name, string address)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (String.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Value must not be empty or whitespace", nameof(name));

            if (address == null)
                throw new ArgumentNullException(nameof(address));

            if (String.IsNullOrWhiteSpace(address))
                throw new ArgumentException("Value must not be empty or whitespace", nameof(address));

            Name = name;
            Address = address;
        }

    }
}