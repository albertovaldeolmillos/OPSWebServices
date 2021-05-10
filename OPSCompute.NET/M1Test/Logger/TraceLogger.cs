using OPS.Comm;
using PDMHelpers;
using System;

namespace M1Test
{
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class TraceLogger : ITraceable
    {
        //private static readonly log4net.Core.Level GeneralLevel = new log4net.Core.Level(50000, "General");

        private readonly Logger logger;

        public ILoggerManager Creator { get; set; }
        public bool Enabled { get; set; } = true;

        public TraceLogger(Type type)
        {
            logger = new Logger(type);

            log4net.Util.LogLog.InternalDebugging = true;
//            log4net.LogManager.GetRepository().LevelMap.Add(GeneralLevel);


            //General("This is Custom Log");
        }

        //public void General(string message)
        //{
        //    Log.Logger.log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, GeneralLevel, message, null);
        //}

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
