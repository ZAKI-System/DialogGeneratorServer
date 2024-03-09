using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer.Exceptions
{
    [Serializable]
    public class RequestDataException : Exception
    {
        public RequestDataException() { }
        public RequestDataException(string message) : base(message) { }
        public RequestDataException(string message, Exception inner) : base(message, inner) { }
        protected RequestDataException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
