using System;
using System.Net;
using System.Reflection;
using OPS.Comm;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// Main class from which all objects are created
	/// </summary>
	public class BecsMain
	{
		static ILogger _logger;

		internal static ILogger Logger
		{
			get { return _logger; }
		}

		/// <summary>
		/// The main entry point for the process or application
		/// </summary>
		public static void Main(string[] args)
		{
			if (args.Length > 0 && args[0].ToUpper() == "-C")
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
			_logger.AddLog("*** BECS Application Started *** ", LoggerSeverities.Info);

			//OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
			//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);

			// Start running
			BecsEngine engine = new BecsEngine();
			try
			{
				engine.Start();
			}
			catch (Exception)
			{
				engine = null;
				_logger.AddLog("An error has occurred. Quitting...", LoggerSeverities.Info);
				_logger = null;
				return;
			}

			// Wait to end
			_logger.AddLog("*** Press Enter to exit ***", LoggerSeverities.Info);
			Console.ReadLine();
			_logger.AddLog("Quitting...", LoggerSeverities.Info);

			// Stop running
			engine.Stop();

			// Clean up before we go home
			GC.Collect();
			GC.WaitForPendingFinalizers();		
		}

		/// <summary>
		/// The main entry point for the process
		/// </summary>
		private static void ServiceMain()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();

            _logger = new Logger(MethodBase.GetCurrentMethod().DeclaringType);

   //         OPS.Comm.Messaging.CommMain.Logger.AddLogMessage += new AddLogMessageHandler(Logger_AddLogMessage);
			//OPS.Comm.Messaging.CommMain.Logger.AddLogException += new AddLogExceptionHandler(Logger_AddLogException);

			System.ServiceProcess.ServiceBase[] ServicesToRun;
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { new BecsAgent() };
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
	}
}
