using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AccountManager.Core.Exceptions
{
    public class RiotClientNotFoundException : Exception
    {
        public RiotClientNotFoundException()
        {
        }

        public RiotClientNotFoundException(string directory) : base($"Riot client was not found in directory {directory}.")
        {
        }

        protected RiotClientNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}
