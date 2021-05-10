using PDMHelpers;
using System;

namespace M1Test
{
    public class LoggerManager : ILoggerManager
    {
        public ITraceable CreateTracer(Type type)
        {
            ITraceable trace = new TraceLogger(type);
            trace.Creator = this;
            return trace;
        }
    }
}
