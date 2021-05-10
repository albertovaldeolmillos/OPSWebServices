using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OPSCompute.Exceptions
{
    [Serializable]
    public class LoadParametersException : Exception
    {
        public LoadParametersException() { }
        public LoadParametersException(string message) : base(message) { }
        public LoadParametersException(string message, Exception innerException) : base(message, innerException) { }

        protected LoadParametersException(SerializationInfo info, StreamingContext context) : base(info, context) {}
    }
}
