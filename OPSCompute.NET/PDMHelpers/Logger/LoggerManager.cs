using System;

namespace PDMHelpers
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
