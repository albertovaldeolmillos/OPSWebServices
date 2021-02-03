using System;
using System.Threading;
using System.Collections;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// A class that buffers incoming messages to be processed
	/// </summary>
	public class Receiver
	{
		#region Public API

		/// <summary>
		/// Signature for handlers of the MessagesAvailable event
		/// </summary>
		public delegate void MessagesAvailableHandler();

		/// <summary>
		/// Private constructor. Always get a receiver instance through
		/// Receiver.GetInstance;
		/// </summary>
		private Receiver()
		{
			_msgQueue = new Queue(10, 5);
		}
		/// <summary>
		/// Handler for the MessageProcessor MessageArrived event
		/// </summary>
		/// <param name="msgId">The new message identifier</param>
		/// <param name="outcome">The outcome of the message (not used)</param>
		/// <param name="msg">The message received</param>
		/// <param name="sender">The message processor that received the message</param>
		/// <param name="srcId">The source unit id</param>
		public void NewMessage(decimal msgId, RequestOutcome outcome, 
			string msg, MessageProcessor sender, string srcId)
		{
			lock (_msgQueue)
			{
				_msgQueue.Enqueue(new ReceivedMessage(msg, sender.Id, srcId));
			}
			if (MessagesAvailable != null)
				ThreadPool.QueueUserWorkItem(
					new WaitCallback(FireMessagesAvailableEventThreadProc));
			else
				CommMain.Logger.AddLog("WARNING: SocketChannel.NewMessage: MessagesAvailable is null.", LoggerSeverities.Error);
		}
		/// <summary>
		/// Read-only property that returns the number of pending
		/// messages
		/// </summary>
		public int PendingMessages
		{
			get
			{
				int retVal = 0;
				lock (_msgQueue)
				{
					retVal = _msgQueue.Count;
				}
				return retVal;
			}
		}
		/// <summary>
		/// Retrives the following message to be processed. null if
		/// there are no pending messages
		/// </summary>
		public ReceivedMessage NextMessage
		{
			get
			{
				ReceivedMessage msg = null;
				lock (_msgQueue)
				{
					msg = (ReceivedMessage) _msgQueue.Dequeue();
				}
				return msg;
			}
		}
		/// <summary>
		/// Returns the only instance of the Receiver class
		/// </summary>
		/// <returns>The only instance of the Receiver class</returns>
		public static Receiver GetInstance()
		{
			if (_instance == null)
				_instance = new Receiver();
			return _instance;
		}
		/// <summary>
		/// Event fired when a new message arrives
		/// </summary>
		public event MessagesAvailableHandler MessagesAvailable;

		#endregion //Public API
		
		#region Private methods

		private void FireMessagesAvailableEventThreadProc(object state)
		{
			MessagesAvailable();
		}

		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// The buffer where incoming messages are stored until their 
		/// processing
		/// </summary>
		private Queue _msgQueue;
		/// <summary>
		/// The only receiver instance
		/// </summary>
		private static Receiver _instance;

		#endregion //Private data members
	}
	
	/// <summary>
	/// A class that represents the messages as they arrive to 
	/// the Receiver
	/// </summary>
	public class ReceivedMessage
	{
		#region Public API

		/// <summary>
		/// Constructor that receives the message body and the indentifier
		/// of the processor through which a reply needs to be routed if
		/// necessary
		/// </summary>
		/// <param name="msg">The message received</param>
		/// <param name="replyToId">The identifier of the message processor
		/// <param name="srcId">The identifier of the unit id
		/// that received the message</param>
		public ReceivedMessage(string msg, string replyToId, string srcId)
		{
			_msg = msg;
			_replyToId = replyToId;
			_srcId = srcId;
		}
		/// <summary>
		/// Constructor that receives the identifier of an ACK message
		/// and the ACK payload
		/// </summary>
		/// <param name="ackMsgId">The identifer of the message</param>
		/// <param name="msg">The payload of the message</param>
		public ReceivedMessage(decimal ackMsgId, string msg)
		{
			_ackMsgId = ackMsgId;
			_msg = msg;
		}
		/// <summary>
		/// Read-only property that returns the body of the message
		/// </summary>
		public string Message
		{
			get { return _msg; }
		}
		/// <summary>
		/// The identifier of the message processor that received the
		/// message
		/// </summary>
		public string ReplyToId
		{
			get { return _replyToId; }
		}
		/// <summary>
		/// Read-only property that returns the identifer of the
		/// message when this object represents the receiving of
		/// an ACK to a previously sent message
		/// </summary>
		public decimal AckMessageId
		{
			get { return _ackMsgId; }
		}
		/// <summary>
		/// The identifier of unit source id
		/// message
		/// </summary>
		public string SrcId
		{
			get { return _srcId; }
		}
		
		#endregion //Public API
		
		#region  Private data members

		private string _msg;
		private string _replyToId;
		private decimal _ackMsgId;
		// cfe SrcId
		private string _srcId;

		#endregion  // Private data members
	}
}
