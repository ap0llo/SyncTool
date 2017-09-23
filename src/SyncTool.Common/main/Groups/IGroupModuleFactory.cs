using Autofac;

namespace SyncTool.Common.Groups
{
    public interface IGroupModuleFactory
    {
        bool IsAddressSupported(string address);

        Module CreateModule();
    }
}
