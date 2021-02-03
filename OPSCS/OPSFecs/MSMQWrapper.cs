using System;
using System.Messaging;

using OPS.Comm;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// Summary description for MSMQWrapper.
	/// </summary>
	public abstract class MSMQWrapper
	{
		// Attributes
		protected MessageQueue _queue = null;

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

		/// <summary>
		/// The queue.
		/// </summary>
		public MessageQueue Queue
		{
			get { return _queue; }
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
		}

		/// <summary>
		/// Sends a message to the queue
		/// </summary>
		/// <param name="header">A MSMQHeader compatible header</param>
		/// <param name="body">The contents of the message</param>
		/// <param name="pm">A Property manager</param>
		public void Send(MSMQHeader header, string body, IPropertyManager pm)
		{
			lock(_queue) 
			{
				Message msg = new Message(body);
				msg.Label = header.ToString();
				msg.TimeToBeReceived = pm.Caducity;
				msg.Priority = pm.Priority;
				_queue.Send(msg);
			}
		}

		/// <summary>
		/// Stops the queue.
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

			// Raise the event
			if (ReceiveCompleted != null)
				ReceiveCompleted(new FecsBecsHeader(msg.Label), msg.Body.ToString());

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
	}
	
	public class BecsFecsInputWrapper : MSMQWrapper
	{
		public BecsFecsInputWrapper() : base () {}
		public BecsFecsInputWrapper(string path) : base (path) {}
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
	}
	
	/// <summary>
	/// Wrapper for MSMQ allowing to receive messages from CC to BECS
	/// </summary>
	public class CcBecsInputWrapper : MSMQWrapper
	{
		public CcBecsInputWrapper() : base () {}
		public CcBecsInputWrapper(string path) : base (path) {}
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

			// Raise the event
			if (ReceiveCompleted != null)
				ReceiveCompleted(new CcBecsHeader(msg.Label), msg.Body.ToString());

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
	}
}
