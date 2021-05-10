using System;

namespace OPS.Comm
{
    public enum LoggerSeverities
    {
        /// <summary>
        /// Only Fatal Error messages are logged. Is the lowest level of logging.
        /// </summary>
        FatalError = 5,
        /// <summary>
        /// Error and FatalError messages will be logged
        /// </summary>
        Error = 4,
        /// <summary>
        /// Warning, Error and FatalError messages will be logged
        /// </summary>
        Warning = 2,
        /// <summary>
        /// Info, Warning, Error and FatalError messages will be logged
        /// </summary>
        Info = 1,
        /// <summary>
        /// All messages will be logged. Is the highest level of logging.
        /// </summary>
        Debug = 0
    }

    public interface ILogger
    {
        void AddLog(string strMessage, LoggerSeverities severity);
        void AddLog(string strMessage, LoggerSeverities severity, Exception excObject);
        void AddLog(string strMessage, Exception excObject);
        void AddLog(Exception excObject);
    }
}