using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SyncTool.Common
{
    public class GroupOpenedException : Exception
    {
        public GroupOpenedException()
        {
        }

        public GroupOpenedException(string message) : base(message)
        {
        }

        public GroupOpenedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GroupOpenedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
