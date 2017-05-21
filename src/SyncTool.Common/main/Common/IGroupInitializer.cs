using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Common
{
    public interface IGroupInitializer
    {

        void Initialize(string groupName, string address);

    }
}
