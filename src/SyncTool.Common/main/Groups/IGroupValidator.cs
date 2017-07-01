using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Common.Groups
{
    public interface IGroupValidator
    {

        void EnsureGroupIsValid(string groupName, string address);

    }
}
