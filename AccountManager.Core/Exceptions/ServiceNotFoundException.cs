
using System.Runtime.Serialization;

namespace AccountManager.Core.Exceptions
{
    [Serializable]
    public class ServiceNotFoundException : Exception
    {
        public ServiceNotFoundException()
        {
        }

        public ServiceNotFoundException(string serviceName) : base($"Service {serviceName} was not found.")
        {
        }

        protected ServiceNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
        {
        }
    }
}
