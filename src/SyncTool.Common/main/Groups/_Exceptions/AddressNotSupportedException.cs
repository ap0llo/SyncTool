using System;

namespace SyncTool.Common.Groups
{
    public class AddressNotSupportedException : Exception
    {
        public string Address { get; }

        public AddressNotSupportedException(string message, string address) : base(message)
        {
            Address = address;
        }
    }
}