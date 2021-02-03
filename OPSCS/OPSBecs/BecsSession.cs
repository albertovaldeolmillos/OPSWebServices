using System;
using OPS.Comm;
using OPS.Comm.Becs.Messages;

namespace OPS.Comm.Becs
{
	public class BecsSession
	{
		// Resource: Message queues (MSMQ)
		protected FecsBecsInputWrapper _fecsBecsInputQueue;
		protected BecsFecsOutputWrapper _becsfecsOutputQueue;
		protected CcBecsInputWrapper _ccBecsInputQueue;
		protected BecsCcOutputWrapper _becsCcOutputQueue;

		// Messages session
		protected OPS.Comm.Becs.Messages.MessagesSession _messagesSession;

		#region Static stuff

		private static string FECS_INPUT_QUEUE_PATH;
		private static string FECS_OUTPUT_QUEUE_PATH;
		private static string CC_INPUT_QUEUE_PATH;
		private static string CC_OUTPUT_QUEUE_PATH;

		static BecsSession()
		{
			System.Configuration.AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
			FECS_INPUT_QUEUE_PATH = (string)appSettings.GetValue("FecsInputQueuePath", typeof(string));
			FECS_OUTPUT_QUEUE_PATH = (string)appSettings.GetValue("FecsOutputQueuePath", typeof(string));
			CC_INPUT_QUEUE_PATH = (string)appSettings.GetValue("CcInputQueuePath", typeof(string));
			CC_OUTPUT_QUEUE_PATH = (string)appSettings.GetValue("CcOutputQueuePath", typeof(string));
		}
		
		#endregion

		/// <summary>
		/// Builds a new Session. A "Session" is just a collection of objects 
		/// maintained in memory by the BECS for performance reasons.
		/// </summary>
		public BecsSession() 
		{
			_fecsBecsInputQueue = new FecsBecsInputWrapper(FECS_INPUT_QUEUE_PATH, BecsMain.Logger);
			_becsfecsOutputQueue = new BecsFecsOutputWrapper(FECS_OUTPUT_QUEUE_PATH, BecsMain.Logger);
			_ccBecsInputQueue = new CcBecsInputWrapper(CC_INPUT_QUEUE_PATH, BecsMain.Logger);
			_becsCcOutputQueue = new BecsCcOutputWrapper(CC_OUTPUT_QUEUE_PATH, BecsMain.Logger);
			_messagesSession = new MessagesSession();
		}

		/// <summary>
		/// Starts a "new" session (empties all information)
		/// </summary>
		public void StartNewSession()
		{
			// Log start
			BecsMain.Logger.AddLog("New BECS session started", LoggerSeverities.Info);

			// Start the asynchronous receive...
			_fecsBecsInputQueue.BeginReceive();
			_ccBecsInputQueue.BeginReceive();
		}

		/// <summary>
		/// Stops the current session
		/// </summary>
		public void StopSession()
		{
			_fecsBecsInputQueue.Close();
			_becsfecsOutputQueue.Close();
			_ccBecsInputQueue.Close();
			_becsCcOutputQueue.Close();

			// Log stop
			BecsMain.Logger.AddLog("BECS session stopped", LoggerSeverities.Info);
		}

		public FecsBecsInputWrapper FecsBecsInputQueue
		{
			get { return _fecsBecsInputQueue; }
		}

		public BecsFecsOutputWrapper BecsFecsOutputQueue
		{
			get { return _becsfecsOutputQueue; }
		}

		public CcBecsInputWrapper CcBecsInputQueue
		{
			get { return _ccBecsInputQueue; }
		}

		public BecsCcOutputWrapper BecsCcOutputQueue
		{
			get { return _becsCcOutputQueue; }
		}

		public MessagesSession MessagesSession
		{
			get { return _messagesSession; }
		}
	}
}
