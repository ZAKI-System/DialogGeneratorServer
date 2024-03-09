using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DialogGeneratorServer.Exceptions
{
    [Serializable]
    public class ProcessException : Exception
    {
        public ProcessException() { }
        public ProcessException(string message) : base(message) { }
        public ProcessException(string message, Exception inner) : base(message, inner) { }
        protected ProcessException(
          SerializationInfo info,
          StreamingContext context) : base(info, context) { }
    }
}
