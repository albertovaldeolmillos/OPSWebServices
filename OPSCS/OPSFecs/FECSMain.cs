using System;
using System.Net;
using System.Reflection;
using OPS.Comm;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Main class from which all objects are created
	/// </summary>
	public class FecsMain
	{
		static ILogger _logger;

		public static ILogger Logger
		{
			get { return _logger; }
		}

		/// <summary>
		/// The main entry point for the process or application
		/// </summary>
		public static void Main(string[] args)
		{
			uint maxWorkerThreads;
			uint maxIOThreads;
			CLRThreadPool.Controller.GetMaxThreads(out maxWorkerThreads, out maxIOThreads);
			maxWorkerThreads = 33;
			CLRThreadPool.Controller.SetMaxThreads(maxWorkerThreads, maxIOThreads);

			if (args.Length > 0 && args[0].ToUpper().Equals("-C"))
				ApplicationMain();
			else
				ServiceMain();
		}

		/// <summary>
		/// It's an application so let's start it all
		/// </summary>
		private static void ApplicationMain()
		{
			// Welcome and Start listening
			_logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);
			_logger.AddLog("*** FECS Application Started *** ", LoggerSeverities.Info);
			/// Determine logging level
			LoggerSeverities logSeverity = ReadLoggerSeverity();
			_logger.AddLog(string.Format("Setting logger severity to {0}: ", logSeverity.ToString()), LoggerSeverities.Info);

			//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
			//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);

			// Start running
			FecsEngine engine = new FecsEngine();
			engine.Start();

			// Wait to end
			_logger.AddLog("*** Press Enter to exit ***", LoggerSeverities.Info);
			Console.ReadLine();
			_logger.AddLog("Quitting...", LoggerSeverities.Info);

			// Stop running
			engine.Stop();

			// Clean up before we go home
			//GC.Collect();
			GC.WaitForPendingFinalizers();		
		}

		/// <summary>
		/// The main entry point for the process
		/// </summary>
		private static void ServiceMain()
		{

			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

            _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);

			/// Determine logging level
			LoggerSeverities logSeverity = ReadLoggerSeverity();
			_logger.AddLog(string.Format("Setting logger severity to: {0} ", logSeverity.ToString()), LoggerSeverities.Info);
			
			//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
			//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);

			System.ServiceProcess.ServiceBase[] ServicesToRun =
				new System.ServiceProcess.ServiceBase[] { new FecsAgent() };
			System.ServiceProcess.ServiceBase.Run(ServicesToRun);
		}

		private static void Logger_AddLogMessage(string msg, LoggerSeverities severity)
		{
			_logger.AddLog(msg, severity);
		}

		private static void Logger_AddLogException(Exception ex)
		{
			_logger.AddLog(ex);
		}

		/// <summary>
		/// Reads logger severity level from the app.config file
		/// </summary>
		private static LoggerSeverities ReadLoggerSeverity()
		{
			LoggerSeverities logSeverity = LoggerSeverities.Error;
			try
			{
				System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				string logLevel = (string)appSettings.GetValue("LoggerSeverity", typeof(string));
				if (logLevel != null)
				{
					logSeverity = (LoggerSeverities) Enum.Parse(
						typeof(LoggerSeverities), logLevel, true);
				}
			}
			catch {}

			return logSeverity;
		}
	}
}
