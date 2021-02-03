using System;
using System.Messaging;
using OPS.Comm.Common;

namespace OPS.Comm.Common
{
	/// <summary>
	/// Summary description for MSMQWrapper.
	/// </summary>
	public abstract class MSMQWrapper
	{
		// Attributes
		protected MessageQueue _queue = null;
		protected ILogger _logger = null;

		public MSMQWrapper()
		{
			// Create the queue
			_queue = new MessageQueue();
			_queue.Formatter = new XmlMessageFormatter(new Type[] {typeof(String)});
		}

		public MSMQWrapper(string path)
		{
			// Create the queue
			_queue = new MessageQueue();
			_queue.Formatter = new XmlMessageFormatter(new Type[] {typeof(String)});
			_queue.Path = path;
		}

		public MSMQWrapper(string path, ILogger logger)
		{
			// Create the queue
			_queue = new MessageQueue();
			_queue.Formatter = new XmlMessageFormatter(new Type[] {typeof(String)});
			_queue.Path = path;
			_logger = logger;
		}

		/// <summary>
		/// Standard queue path property.
		/// </summary>
		public string Path
		{
			get { return _queue.Path; }
			set { _queue.Path = value; }
		}

		/// <summary>
		/// The queue.
		/// </summary>
		public MessageQueue Queue
		{
			get { return _queue; }
		}
	
		/// <summary>
		/// The logger.
		/// </summary>
		public ILogger Logger
		{
			get { return _logger; }
			set { _logger = value; }
		}

		/// <summary>
		/// Sends a message to the queue
		/// </summary>
		/// <param name="header">A MSMQHeader compatible header</param>
		/// <param name="body">The contents of the message</param>
		public void Send(MSMQHeader header, string body)
		{
			lock(_queue) 
			{
				_queue.Send(body, header.ToString());
			}
			if (_logger != null)
				_logger.AddLog("Sending message [" + body + "] with header [" + header.ToString() + "]", LoggerSeverities.Debug);
		}

		/// <summary>
		/// Deletes all messages contained in the queue.
		/// </summary>
		public void Purge()
		{
			_queue.Purge();
		}

		/// <summary>
		/// Frees all resources allocated by the queue.
		/// </summary>
		public void Close()
		{
			_queue.Close();
		}
	}

	/// <summary>	
	/// Wrapper for MSMQ allowing to receive messages from FECS to BECS
	/// </summary>
	public class FecsBecsInputWrapper : MSMQWrapper
	{
		public FecsBecsInputWrapper() : base () {}
		public FecsBecsInputWrapper(string path) : base (path) {}
		public FecsBecsInputWrapper(string path, ILogger logger) : base (path, logger) {}
		public delegate void ReceiveCompletedHandler(FecsBecsHeader header, string body);
		public event ReceiveCompletedHandler ReceiveCompleted;

		/// <summary>
		/// Starts the reception of messages in the queue.
		/// </summary>
		public void BeginReceive()
		{
			// Configure callback
			_queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompleted);
			_queue.BeginReceive();
		}

		/// <summary>
		/// Handler of a message received (acts as a callback)		
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
		{
			// Get the data
			MessageQueue mq = (MessageQueue)sender;
			Message msg = mq.EndReceive(e.AsyncResult);
			if (_logger != null)
				_logger.AddLog("Receiving message [" + msg.Body.ToString() + "] with header [" + msg.Label + "]", LoggerSeverities.Debug);

			// Raise the event
			if (ReceiveCompleted != null)
			{
				try
				{
					ReceiveCompleted(new FecsBecsHeader(msg.Label), msg.Body.ToString());
				}
				catch (Exception ex)
				{
					if (_logger != null) 
						_logger.AddLog(ex);
				}
			}

			// Restart the reception
			mq.BeginReceive();
		}
	}

	/// <summary>
	/// Wrapper for MSMQ allowing to send messages from FECS to BECS
	/// </summary>
	public class FecsBecsOutputWrapper : MSMQWrapper
	{
		public FecsBecsOutputWrapper() : base () {}
		public FecsBecsOutputWrapper(string path) : base (path) {}
		public FecsBecsOutputWrapper(string path, ILogger logger) : base (path, logger) {}
	}
	
	public class BecsFecsInputWrapper : MSMQWrapper
	{
		public BecsFecsInputWrapper() : base () {}
		public BecsFecsInputWrapper(string path) : base (path) {}
		public BecsFecsInputWrapper(string path, ILogger logger) : base (path, logger) {}
		public delegate void ReceiveCompletedHandler(BecsFecsHeader header, string body);
		public event ReceiveCompletedHandler ReceiveCompleted;

		/// <summary>
		/// Starts the reception of messages in the queue.
		/// </summary>
		public void BeginReceive()
		{
			// Configure callback
			_queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompleted);
			_queue.BeginReceive();
		}

		/// <summary>
		/// Handler of a message received (acts as a callback)		
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
		{
			// Get the data
			MessageQueue mq = (MessageQueue)sender;
			Message msg = mq.EndReceive(e.AsyncResult);
			if (_logger != null)
				_logger.AddLog("Receiving message [" + msg.Body.ToString() + "] with header [" + msg.Label + "]", LoggerSeverities.Debug);

			// Raise the event
			if (ReceiveCompleted != null)
				ReceiveCompleted(new BecsFecsHeader(msg.Label), msg.Body.ToString());

			// Restart the reception
			mq.BeginReceive();
		}
	}

	/// <summary>
	/// Wrapper for MSMQ allowing to send messages from BECS to FECS
	/// </summary>
	public class BecsFecsOutputWrapper : MSMQWrapper
	{
		public BecsFecsOutputWrapper() : base () {}
		public BecsFecsOutputWrapper(string path) : base (path) {}
		public BecsFecsOutputWrapper(string path, ILogger logger) : base (path, logger) {}
	}
	
	/// <summary>
	/// Wrapper for MSMQ allowing to receive messages from CC to BECS
	/// </summary>
	public class CcBecsInputWrapper : MSMQWrapper
	{
		public CcBecsInputWrapper() : base () {}
		public CcBecsInputWrapper(string path) : base (path) {}
		public CcBecsInputWrapper(string path, ILogger logger) : base (path, logger) {}
		public delegate void ReceiveCompletedHandler(CcBecsHeader header, string body);
		public event ReceiveCompletedHandler ReceiveCompleted;

		/// <summary>
		/// Starts the reception of messages in the queue.
		/// </summary>
		public void BeginReceive()
		{
			// Configure callback
			_queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompleted);
			_queue.BeginReceive();
		}

		/// <summary>
		/// Handler of a message received (acts as a callback)		
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
		{
			// Get the data
			MessageQueue mq = (MessageQueue)sender;
			Message msg = mq.EndReceive(e.AsyncResult);
			if (_logger != null)
				_logger.AddLog("Receiving message [" + msg.Body.ToString() + "] with header [" + msg.Label + "]", LoggerSeverities.Debug);

			// Raise the event
			if (ReceiveCompleted != null)
				ReceiveCompleted(new CcBecsHeader(msg.Label), msg.Body.ToString());

			// Restart the reception
			mq.BeginReceive();
		}
	}

	/// <summary>
	/// Wrapper for MSMQ allowing to send messages from CC to BECS
	/// </summary>
	public class CcBecsOutputWrapper : MSMQWrapper
	{
		public CcBecsOutputWrapper() : base () {}
		public CcBecsOutputWrapper(string path) : base (path) {}
		public CcBecsOutputWrapper(string path, ILogger logger) : base (path, logger) {}
	}

	/// <summary>
	/// Wrapper for MSMQ allowing to receive messages from BECS to CC
	/// </summary>
	public class BecsCcInputWrapper : MSMQWrapper
	{
		public BecsCcInputWrapper() : base () {}
		public BecsCcInputWrapper(string path) : base (path) {}
		public BecsCcInputWrapper(string path, ILogger logger) : base (path, logger) {}
		public delegate void ReceiveCompletedHandler(BecsCcHeader header, string body);
		public event ReceiveCompletedHandler ReceiveCompleted;

		/// <summary>
		/// Starts the reception of messages in the queue.
		/// </summary>
		public void BeginReceive()
		{
			// Configure callback
			_queue.ReceiveCompleted += new ReceiveCompletedEventHandler(queue_ReceiveCompleted);
			_queue.BeginReceive();
		}

		/// <summary>
		/// Handler of a message received (acts as a callback)		
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void queue_ReceiveCompleted(object sender, ReceiveCompletedEventArgs e)
		{
			// Get the data
			MessageQueue mq = (MessageQueue)sender;
			Message msg = mq.EndReceive(e.AsyncResult);
			if (_logger != null)
				_logger.AddLog("Receiving message [" + msg.Body.ToString() + "] with header [" + msg.Label + "]", LoggerSeverities.Debug);

			// Raise the event
			if (ReceiveCompleted != null)
			{
				try
				{
					ReceiveCompleted(new BecsCcHeader(msg.Label), msg.Body.ToString());
				}
				catch (Exception ex)
				{
					if (_logger != null) 
						_logger.AddLog(ex);
				}
			}

			// Restart the reception
			mq.BeginReceive();
		}
	}

	/// <summary>
	/// Wrapper for MSMQ allowing to send messages from BECS to CC
	/// </summary>
	public class BecsCcOutputWrapper : MSMQWrapper
	{
		public BecsCcOutputWrapper() : base () {}
		public BecsCcOutputWrapper(string path) : base (path) {}
		public BecsCcOutputWrapper(string path, ILogger logger) : base (path, logger) {}
	}
}
