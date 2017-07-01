namespace SyncTool.Common.Groups
{
    public interface IGroupValidator
    {
        void EnsureGroupIsValid(string groupName, string address);
    }
}
