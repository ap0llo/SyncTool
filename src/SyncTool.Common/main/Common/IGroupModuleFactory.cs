using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace SyncTool.Common
{
    public interface IGroupModuleFactory
    {
        Module CreateModule();
    }
}
