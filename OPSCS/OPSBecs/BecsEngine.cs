using System;
using System.Data;
using System.Messaging;
using System.Threading;
using System.Collections;
using OPS.Comm;
using OPS.Comm.Messaging;
using OPS.Components;
using OPS.Components.Data;

namespace OPS.Comm.Becs
{
	/// <summary>
	/// Summary description for BecsEngine.
	/// </summary>
	public class BecsEngine
	{
		#region Static stuff
		private static BecsSession _session;
		private static BecsEngine _mainEngine;

		static BecsEngine()
		{
			_session = new BecsSession();
			_mainEngine = null;
		}

		public static BecsSession Session
		{
			get { return _session; }
		}

		public static BecsEngine MainEngine
		{
			get { return _mainEngine; }
		}
		#endregion

		protected bool _ending = true;

		//protected IMsgDispatcher _msgDispatcher;

		// That hashtable contains all active threads.
		// Each thread handles the response of a message to a client.
		// Every time that a message is received a thread is creates to process that message and send the response
		// back to the msmq
//		protected Hashtable _threads;
		protected CommObjects _commObjects;

		/// <summary>
		/// Constructs a new BecsEngine
		/// </summary>
		public BecsEngine()
		{
			if (_mainEngine == null)
				_mainEngine = this;
		}

		public void Start()
		{
//			_threads = new Hashtable();
			_ending = false;

			//_session.FecsBecsInputQueue.ReceiveCompleted += new OPS.Comm.Common.FecsBecsInputWrapper.ReceiveCompletedHandler(OnNewFecsMessage);

			//_session.CcBecsInputQueue.ReceiveCompleted += new OPS.Comm.Common.CcBecsInputWrapper.ReceiveCompletedHandler(OnNewCcMessage);

			// Starts the dispatcher process...
			//_msgDispatcher = MsgDispatcher.GetMessageDispatcher();
			//BecsMain.Logger.AddLog("Starting MessageDispatcher Thread", LoggerSeverities.Info);
			//			_msgDispatcher.Queue = _fecsOutputQueue;
			//_msgDispatcher.Queue = _session.BecsFecsOutputQueue.Queue;
			// TODO: Reactivate msgDIspatcher
			// _msgDispatcher.Start();
			//BecsMain.Logger.AddLog("MessageDispatcher Thread started", LoggerSeverities.Info);
			//_msgDispatcher.Logger = BecsMain.Logger;

			/// ///////////////////////////////////////////
			
			// Check database connection
			try
			{
				Database d = OPS.Components.Data.DatabaseFactory.GetDatabase ();
				OPS.Components.Data.DatabaseFactory.Logger = BecsMain.Logger;

			}
			catch (Exception ex)
			{
				// Record exception
				BecsMain.Logger.AddLog(ex);

				// Clean up
				_commObjects.Shutdown();
				_commObjects = null;
				_session.StopSession();

				// Throw up exception
				throw ex;
			}

			_commObjects = new CommObjects();
			_commObjects.Initialize(_session.FecsBecsInputQueue, _session.BecsFecsOutputQueue);
			
			/// Delete when done with the message send test
			//ThreadPool.QueueUserWorkItem(new WaitCallback(GenerateMessageThreadProc));

			// Init the session...
			_session.StartNewSession();
		}

		/// <summary>
		/// Stops the BECS - Processing of messages is also aborted(so, the messages ARE lost).
		/// </summary>
		public void Stop()
		{
			_ending = true;
			BecsMain.Logger.AddLog("BECS process stopping...", LoggerSeverities.Info);

			//_msgDispatcher.Queue  = null;
			// TODO: Reactivate msgdispatcher
			//_msgDispatcher.Stop(null);

			/// Delete when done with the message send test
			_endEvt.Set();
			/// ///////////////////////////////////////////

			_commObjects.Shutdown();

			_session.StopSession();

//			foreach(DictionaryEntry de in _threads)
//			{
//				BecsMain.Logger.AddLog("Aborting thread " + de.Key.ToString(), LoggerSeverities.Info);
//				((ProcessThread)de.Value).Thread.Abort();
//			}
//			_threads.Clear();
		}

		public CommObjects CommObjects
		{
			get { return _commObjects; }
		}

//		/// <summary>
//		/// Removes the current thread from hashtable
//		/// </summary>
//		/// <param name="t"></param>
//		public void RemoveThread(ProcessThread t)
//		{
//			if(_threads.ContainsKey(t.GetHashCode()))
//			{
//				_threads.Remove(t.GetHashCode());
//				BecsMain.Logger.AddLog("Thread " + t.GetHashCode() + " finished and cleaned up", LoggerSeverities.Info);
//			}
//		}

//		// Function called each time a new message from FECS is recieved
//		private void OnNewFecsMessage(FecsBecsHeader header, string body)
//		{
//			if(!_ending) 
//			{
//				ProcessThread pt = new ProcessThread(header, body, _session.BecsFecsOutputQueue, this);
//				ThreadStart ts  = new ThreadStart(pt.Process);	
//				System.Threading.Thread t = new System.Threading.Thread(ts);
//				_threads.Add(t.GetHashCode(), pt); // We add the process thread with the HashCode of the Thread object.
//				t.Start();
//			}
//		}

		// Function called each time a new message from CC is recieved
		private void OnNewCcMessage(CcBecsHeader header, string body)
		{
//			if(!_ending) 
//			{
//				ProcessThread pt = new ProcessThread(header, body, _session.BecsCcOutputQueue, this);
//				ThreadStart ts  = new ThreadStart(pt.Process);	
//				System.Threading.Thread t = new System.Threading.Thread(ts);
//				_threads.Add(t.GetHashCode(), pt); // We add the process thread with the HashCode of the Thread object.
//				t.Start();
//			}
		}

		#region Send messages test - delete when done with the test

		private ManualResetEvent _endEvt = new ManualResetEvent(false);
		
		private void GenerateMessageThreadProc(object args)
		{
			BecsMain.Logger.AddLog("Test send thread started", LoggerSeverities.Debug);
			bool exit = false;
			while (!exit)
			{
				if (_endEvt.WaitOne(60000, false))
				{
					/// the thread must stop
					exit = true;
				}
				else
				{
					/// The waiting timeout expired
					GenerateMessageToSend();
				}
			}
			BecsMain.Logger.AddLog("Test send thread stopped", LoggerSeverities.Debug);
		}
		private void GenerateMessageToSend()
		{
			decimal srcUnit = decimal.Parse(_commObjects.UnitId);
			decimal dstUnit = 2;
			try
			{
				DataSet cfgData = _commObjects.MessageConfiguration.GetConfiguration(
					Tags.MailMsg, srcUnit, dstUnit);
				if (cfgData != null)
				{
					string msg = string.Format("<m51 id=\"0\" ret=\"0\" dst=\"0\" pvt=\"1\"><e>{0}</e></m51>", 
						DateTime.Now.ToLongTimeString());
					CmpMessages cmpMsg = new CmpMessages();
					cmpMsg.CreateFromConfig(Tags.MailMsg, srcUnit, dstUnit, cfgData, msg);
				}
				else
				{
					BecsMain.Logger.AddLog("GenerateMessageToSend - No configuration for " + Tags.MailMsg,
						LoggerSeverities.Debug);
				}
			}
			catch (DataException dex)
			{
				BecsMain.Logger.AddLog("GenerateMessageToSend data exception:", LoggerSeverities.Debug);
				if (dex.InnerException != null)
					BecsMain.Logger.AddLog(dex.InnerException);
				else
					BecsMain.Logger.AddLog(dex);
			}
			catch (Exception ex)
			{
				BecsMain.Logger.AddLog("GenerateMessageToSend exception:", LoggerSeverities.Debug);
				BecsMain.Logger.AddLog(ex);
			}
		}

		#endregion
	}
}
