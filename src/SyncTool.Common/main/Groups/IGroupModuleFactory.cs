using Autofac;

namespace SyncTool.Common.Groups
{
    public interface IGroupModuleFactory
    {
        Module CreateModule();
    }
}
