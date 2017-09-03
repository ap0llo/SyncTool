using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Sql.Model
{
    public sealed class DatabaseLimits
    {
        /// <summary>
        /// Gets the maximum number of parameters that can be used in a single command
        /// </summary>
        public int MaxParameterCount { get; }


        public DatabaseLimits(int maxParameterCount)
        {
            MaxParameterCount = maxParameterCount;
        }
    }
}
