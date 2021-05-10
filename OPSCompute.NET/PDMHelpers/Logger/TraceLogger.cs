using OPS.Comm;
using System;
using System.Runtime.CompilerServices;

namespace PDMHelpers
{
    public class TraceLogger : ITraceable
    {

        private readonly Logger logger;

        public ILoggerManager Creator { get; set; }
        public bool Enabled { get; set; } = true;

        public TraceLogger(Type type)
        {
            logger = new Logger(type);

            log4net.Util.LogLog.InternalDebugging = true;
        }

        public void Write(TraceLevel level, string trace, [CallerMemberName] string caller = null)
        {
            if (Enabled) {
                var callerM = new System.Diagnostics.StackTrace(1, false).GetFrame(0).GetMethod();

                logger.AddLog($"{callerM.DeclaringType.Name}.{callerM.Name} - {trace}", (LoggerSeverities)level);
            }
        }

        public void Write(TraceLevel level, string context, string trace, [CallerMemberName] string caller = null)
        {
            if (Enabled) {
                logger.AddLog($"{context} - {caller} :: {trace}", (LoggerSeverities)level);
            }
        }
    }
}
