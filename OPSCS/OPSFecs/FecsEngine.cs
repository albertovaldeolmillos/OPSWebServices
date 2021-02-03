using System;
using System.Net;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Main working process.
	/// </summary>
	public class FecsEngine
	{
		// Queue to BECS
		protected FecsBecsOutputWrapper _fecsBecsOutputQueue = null;
		// Queue from BECS
		protected BecsFecsInputWrapper _becsFecsInputQueue = null;

		#region Static stuff

		private static int PORT_NUMBER;
		private static string OUTPUT_QUEUE_PATH;
		private static string INPUT_QUEUE_PATH;
		private static string XML_SCHEMAS_PATH;

		static FecsEngine()
		{
			// Read configuration values
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			PORT_NUMBER = int.Parse((string)appSettings.GetValue("PortNumber", typeof(string)));
			OUTPUT_QUEUE_PATH = (string)appSettings.GetValue("OutputQueuePath", typeof(string));
			INPUT_QUEUE_PATH = (string)appSettings.GetValue("InputQueuePath", typeof(string));
			XML_SCHEMAS_PATH = (string)appSettings.GetValue("XMLSchemasPath", typeof(string));
		}

		#endregion

		public FecsEngine() {}

		/// <summary>
		/// Launches listening to clients and BECS.
		/// Call it to really start the functionality of the class.
		/// </summary>
		public void Start()
		{
			// Create the queue sender to BECS
			_fecsBecsOutputQueue = new FecsBecsOutputWrapper(OUTPUT_QUEUE_PATH);
			// Create the queue receiver from BECS
			_becsFecsInputQueue = new BecsFecsInputWrapper(INPUT_QUEUE_PATH);

			// Determine the IPAddress of this machine
			IPAddress [] aryLocalAddr = null;
			String strHostName = "";
			try
			{
				// NOTE: DNS lookups are nice and all but quite time consuming.
				strHostName = Dns.GetHostName();
				IPHostEntry ipEntry = Dns.GetHostEntry(strHostName);
				aryLocalAddr = ipEntry.AddressList;
			}
			catch(Exception ex)
			{
				//Logger.WriteLogException(3, "Error trying to get local address", ex);
				FecsMain.Logger.AddLog(ex);
			}
	
			// Verify we got an IP address. Tell the user if we did
			if(aryLocalAddr == null || aryLocalAddr.Length < 1)
			{
				//Logger.WriteLogMessage(3, "Unable to get local address");
				FecsMain.Logger.AddLog("Unable to get local address", LoggerSeverities.Error);
				return;
			}
			/// JLB: GPRS -> Use second address if available
			/// TODO: Remove when testing base connection
			//int index =(aryLocalAddr.Length > 1) ? 1 : 0;

			// Initialize listener
			try
			{
				CommObjects.Initialize(PORT_NUMBER, XML_SCHEMAS_PATH, 
					_becsFecsInputQueue, _fecsBecsOutputQueue);
			}
			catch(Exception ex)
			{
				//Logger.WriteLogMessage(3, "Port is busy. Can't create listener on : {0}:{1} - {2}", aryLocalAddr[index], int.Parse(PORT_NUMBER), ex.Message);
				FecsMain.Logger.AddLog("Port is busy. Can't create listener on port : " + PORT_NUMBER + " - " + ex.Message, LoggerSeverities.Error);
				return;
			}

			// Notify about correct working
			//Logger.WriteLogMessage(3, "Listening on : [{0}] {1}:{2}", strHostName, aryLocalAddr[index], int.Parse(PORT_NUMBER));
			FecsMain.Logger.AddLog("Listening on : [" + strHostName + ":" + PORT_NUMBER + "]", LoggerSeverities.Info);

			// Initialize queue
			try
			{
				_becsFecsInputQueue.BeginReceive();
			}
			catch(Exception ex)
			{
				//Logger.WriteLogMessage(3, "Wrong queue path(either input or output) - {0}", ex.Message);
				FecsMain.Logger.AddLog(ex);
				return;
			}
		}

		/// <summary>
		/// Stops listening to new connections.
		/// Call it to end the functionality of the class.
		/// </summary>
		public void Stop()
		{
			CommObjects.Shutdown();
			if (_fecsBecsOutputQueue != null)
			{
				_fecsBecsOutputQueue.Close();
				_fecsBecsOutputQueue = null;
			}
			if (_becsFecsInputQueue != null)
			{
				_becsFecsInputQueue.Close();
				_becsFecsInputQueue = null;
			}
		}
	}
}
