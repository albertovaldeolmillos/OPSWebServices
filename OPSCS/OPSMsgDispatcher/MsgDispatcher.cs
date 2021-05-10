using System;
using System.Threading;
using System.Messaging;
using System.Data;
using OPS.Comm;
using OPS.Components.Data;

namespace OPS.Comm.Becs.Messages
{

	public delegate void StopCallback ();
	/// <summary>
	/// Interface that defines all methods that a Message-dispatcher class must have.
	/// </summary>
	public interface IMsgDispatcher
	{
		/// <summary>
		/// Starts the dispatcher process
		/// </summary>
		void Start();

		/// <summary>
		/// Stops the dispatcher process.
		/// </summary>
		/// <param name="callback">Callback method to invoke just BEFORE the thread ends</param>
		void Stop(StopCallback callback);

		/// <summary>
		/// Gets or sets the time slice.
		/// </summary>
		int SliceTime { set; get; }

		/// <summary>
		/// Gets or sets the MessageQueue used to send message back to FECS
		/// </summary>
		MessageQueue Queue { set; get; }

		/// <summary>
		/// Sets the ILogger object used
		/// </summary>
		ILogger Logger { set; }
	}

	/// <summary>
	/// Message-dispatcher class (singleton class)
	/// There's only one object of that class, and runs on his own thread.
	/// The only-one object of that class watches for changes or updates in MSG table of the Database
	/// and adds the corresponding entry into the becsfecs msmq queue.
	/// </summary>
	public class MsgDispatcher : IMsgDispatcher
	{
		// static var that contains the only-one object
		protected static IMsgDispatcher _this;
		
		private Thread _thread;
		private int _slice;
		private bool _running;
		private StopCallback _callback;
		private DataSet _ds;
		private ILogger _logger;
		private MessageQueue _queue;

		#region Singleton pattern implementation
		/// <summary>
		/// Constructs a new MsgDispatcher (only derived classes can call ctor)
		/// </summary>
		protected MsgDispatcher() 
		{
			_slice = 3000;	// Default 3 s time for process interval
			_ds = null;
			_thread = new Thread (new ThreadStart (DoProcess));
			_queue =  null;
			_running = false;
			_logger = null;
		}				
		/// <summary>
		/// Gets a reference to the one and only MsgDispatcher
		/// </summary>
		/// <returns>A MsgDispatcher object (implementing IMsgDispatcher interface)</returns>
		public static IMsgDispatcher GetMessageDispatcher()
		{
			if (_this == null) _this = new MsgDispatcher();
			return _this;
		}
		#endregion
			
		#region IMsgDispatcher Members

		/// <summary>
		/// Starts the thread
		/// </summary>
		public void Start()
		{
			if (_logger!=null) _logger.AddLog ("MSG_D::Start() called - current slice time (ms) is is " + _slice,LoggerSeverities.Debug);
			if (!_running) 
			{
				_running = true;
				_thread.Start();
				if (_logger!=null) _logger.AddLog ("MSG_D::Thread started", LoggerSeverities.Debug);
			}

		}

		/// <summary>
		/// Stops the thread
		/// </summary>
		public void Stop(StopCallback callback)
		{
			lock (this)
			{
				if (!_running) return;
				_callback = callback;
				_running = false;
			}
			
		}

		public int SliceTime
		{
			get { return _slice; }
			set { _slice = value; }
		}

		public ILogger Logger
		{
			set { _logger = value;}
		}

		public MessageQueue Queue
		{
			get { return _queue; }
			set 
			{ 
				_queue = value; 
			}
		}

		#endregion


		/// <summary>
		/// Thread on that runs the only-one instance of the class... ;)
		/// While running processes messages on a slice time interval.
		/// </summary>
		protected virtual void DoProcess()
		{
			while (_running)
			{
				ProcessMessages();
				if (_logger!=null)
					_logger.AddLog ("MSG_D::Pending messages processed. Waiting <slice> and repeating process",LoggerSeverities.Debug);
				Thread.Sleep (_slice);
			}
			if (_callback!=null) _callback(); 
		}

		/// <summary>
		/// Method that process the MSG table.
		/// Reads the unprocessed messages, and sends them to the becsfecs queue.
		/// </summary>
		protected virtual void ProcessMessages ()
		{
			if (_logger!=null) _logger.AddLog ("MSG_D::Processing pending messages.",LoggerSeverities.Debug);
			if (_queue == null) return;
			try 
			{
				CmpMessagesDB cmp = new CmpMessagesDB();
				// At the first iteration _ds is NULL, so we get ALL messages (SENDING and PENDING).
				// At the rest of iterations only PENDING messages will be retrieved each time...
				_ds = cmp.GetPendingMessages(_ds == null);
				// Process one-on-one the messages of the table...
				foreach (DataRow dr in _ds.Tables[0].Rows)
				{
					MessageFromBD msg = ProcessMessage(dr);
					msg.UpdateData();
				}
				// At that point data on DataSet _ds has been updated in dataset, so we update the DataBase.
				cmp.UpdateMessages (_ds);
			}
			catch (Exception e)
			{
				if (_logger != null) _logger.AddLog (e);
			}
		}

		/// <summary>
		/// Processes ONE pending messsage.
		/// The message is marked as MSG_STATUS sending (1) and is put in the becsfecs queue.
		/// </summary>
		/// <param name="dr">DataRow with info about the message to be sended</param>
		/// <returns>MessageFromBD object containing info about the message to be send</returns>
		protected virtual MessageFromBD ProcessMessage (DataRow dr)
		{
			MessageFromBD msg = null;
			try 
			{
				// Get info of the message, and sends it to the fecsbecs queue.
				msg = new MessageFromBD (dr);
				msg.Send (_queue);
				return msg;
			}
			catch (Exception)
			{
				// Some exception processing the message
				// TODO: Log the exception
				return msg;
			}
		}
	}
}
