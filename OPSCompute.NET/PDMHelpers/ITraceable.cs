using System.Runtime.CompilerServices;

namespace PDMHelpers
{
    public enum TraceLevel {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 4,
        FatalError = 5
    }

    public interface ITraceable
    {
        ILoggerManager Creator { get; set; }
        bool Enabled { get; set; }

        void Write(TraceLevel level, string trace, [CallerMemberName] string caller = null);
        void Write(TraceLevel level, string context, string trace, [CallerMemberName] string caller = null);
    }
}
