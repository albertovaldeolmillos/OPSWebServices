using System;
using System.Collections;
using System.Data;
using System.Diagnostics;
using OPS.Comm;
using OPS.Comm.Media;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Accepts messages and physically sends them in a reliable manner.
	/// </summary>
	public class Sender
	{
		#region Public API

		/// <summary>
		/// The signature for the MessageDone event handlers
		/// </summary>
		/// <param name="msgDataSet">The updated data for the message after
		/// going through the sending process. The dataset has the structure 
		/// described in <see cref="Sender.Send">Sender.Send</see></param>
		public delegate void MessageDoneHandler(DataSet msgDataSet);
		/// <summary>
		/// The signature for the MessageArrived event handlers
		/// </summary>
		public delegate void MessageArrivedHandler(decimal msgId, 
			RequestOutcome outcome, string response, Sender sender, string srcId);

		public Sender(ChannelManager chMgr)
		{
			_channelMgr = chMgr;
			_originId = "???";
		}
		/// <summary>
		/// Sends the contents of the dataset as one or more messages
		/// </summary>
		/// <param name="msgDataSet">It has the structure of the MSGS table; that is:
		/// 
		///	MSG_ID               NUMBER  NOT NULL,
		///	MSG_DMSG_ID          NUMBER  NOT NULL,
		///	MSG_MMSG_ID          NUMBER  NOT NULL,
		///	MSG_DATE             DATE    NOT NULL,
		///	MSG_PRIORITY         NUMBER  NOT NULL,
		///	MSG_MANDATORY        NUMBER  NOT NULL,
		///	MSG_MSG_ID           NUMBER,
		///	MSG_MSG_ORDER        NUMBER,
		///	MSG_XML              VARCHAR2(255 BYTE),
		///	MSG_UNI_ID           NUMBER,
		///	MSG_IPADAPTER        VARCHAR2(20 BYTE),
		///	MSG_PORTADAPTER      NUMBER,
		///	MSG_STATUS           NUMBER  NOT NULL,
		///	MSG_NUMRETRIES       NUMBER,
		///	MSG_LASTRETRY        DATE,
		///	MSG_TOTALRETRIES     NUMBER  NOT NULL,
		///	MSG_PARCIALRETRIES   NUMBER  NOT NULL,
		///	MSG_TOTALINTERVAL    NUMBER  NOT NULL,
		///	MSG_PARCIALINTERVAL  NUMBER  NOT NULL,
		///	MSG_TOTALTIME        NUMBER  NOT NULL,
		///	MSG_HISMANDATORY     NUMBER  DEFAULT 1 NOT NULL,
		///	MSG_FID              NUMBER,
		///	MSG_VERSION          NUMBER,
		///	MSG_VALID            NUMBER DEFAULT 1 NOT NULL,
		///	MSG_DELETED          NUMBER DEFAULT 0  NOT NULL,
		///	CONSTRAINT PK_MSGS PRIMARY KEY (MSG_ID)
		/// 
		/// </param>
		/// <remarks>All messages have the same destination and
		/// priority</remarks>
		public void Send(DataSet msgDataSet)
		{
			_msgDataSet = msgDataSet;
			_messages = new MessagesObject(_msgDataSet);
			_requestsInProcess = 0;
			ArrayList msgGroups = _messages.GetRetryGroups();
			if (msgGroups != null && msgGroups.Count > 0)
			{
				_processorsInUse = new MessageProcessor[msgGroups.Count];
				foreach (RetryGroup g in msgGroups)
				{
					MessageProcessor proc = 
						MessageProcessorManager.GetProcessor(g.Policy);
					if (null == proc)
					{
						IChannel ch = _channelMgr.OpenChannel(g.Policy.URI);
						if (ch != null)
						{
							proc = new MessageProcessor(ch);
							proc.Policy = g.Policy;
							MessageProcessorManager.AddProcessor(proc);
						}
					}
					if (proc != null)
					{
						RegisterProcessor(proc);
						proc.Send(g.Messages);
						_requestsInProcess++;
					}
				}
			}
			else
			{
				//TODO: Done!
			}
		}
		/// <summary>
		/// The event fired after the processing of messages is finished
		/// </summary>
		public event MessageDoneHandler MessageDone
		{
			add { _msgDoneHandler += value; }
			remove { _msgDoneHandler -= value; }
		}
		/// <summary>
		/// The event fired after a message has arrived
		/// </summary>
		public event MessageArrivedHandler MessageArrived
		{
			add { _msgArrivedHandler += value; }
			remove { _msgArrivedHandler -= value; }
		}
		/// <summary>
		/// Property to establish the object to query for media availability
		/// </summary>
		public CommMediaContext MediaContext
		{
			set { _mediaCtx = value; }
			get { return _mediaCtx; }
		}
		/// <summary>
		/// An object that knows how to handle the responses to requests
		/// made from this sender
		/// </summary>
		public IMessageRouter ResponseRouter
		{
			get { return _responseRouter; }
			set { _responseRouter = value; }
		}
		/// <summary>
		/// An identifier of the logical device that sends requests
		/// through this sender
		/// </summary>
		public string OriginId
		{
			get { return _originId; }
			set { _originId = value; }
		}

		#endregion //Public API
		
		#region Private methods

		/*
				public delegate void MessageArrivedHandler(decimal msgId,
					RequestOutcome outcome, OPSTelegrama response, 
					MessageProcessor sender);
				/// <summary>
				/// The signature for handlers of the Done event
				/// </summary>
				public delegate void DoneHandler(MessageProcessor sender);
				/// <summary>
				/// The signature for handlers of the MessageRetried event
				/// </summary>
				public delegate void MessageRetriedHandler(decimal msgId, 
					decimal retriesLeft, MessageProcessor sender);
 
		*/
		/// <summary>
		/// Handler for the MessageProcessor.MessageArrived event
		/// </summary>
		/// <param name="msgId">The identifier of the message arrived</param>
		/// <param name="outcome">The outcome of the sending process</param>
		/// <param name="response">The contents of the response when the outcome
		/// indicates success, null otherwise</param>
		/// <param name="sender">The processor that received the message</param>
		protected void OnRequestResponse(decimal msgId, 
			RequestOutcome outcome, string response,
			MessageProcessor sender, string srcId)
		{
			string text = string.Format("Sender - Updating row({0}): {1}, {2}", 
				msgId, outcome, (response != null)? response : String.Empty);
			Debug.WriteLine(text);
			if (response != null)
			{
				if (_messages.UpdateMessageStatus(
					msgId, (decimal)(int) GetMessageStatus(outcome)))
				{
					if (_responseRouter != null)
						_responseRouter.RouteAck(msgId, response);
				}
			}
		}
		/// <summary>
		/// Handler of the MessageProcessor.RequestDone event
		/// </summary>
		/// <param name="sender">The sender of the event</param>
		protected void OnRequestDone(MessageProcessor sender)
		{
			_requestsInProcess--;
			if (_requestsInProcess == 0)
				_messages.FailPendingMessages();
			Debug.WriteLine("Sender - OnRequestDone");
			if (_responseRouter != null)
				_responseRouter.RouteResponses(_msgDataSet);
			UnregisterProcessor(sender);
		}
		/// <summary>
		/// Handler for the MessageProcessor.MessageRetried event
		/// </summary>
		/// <param name="msgId">The message retried</param>
		/// <param name="retriesLeft">The number of retries left for
		/// the message</param>
		/// <param name="sender">The sender of the event</param>
		protected void OnMessageRetried(decimal msgId, decimal retriesLeft, 
			MessageProcessor sender)
		{
			_messages.UpdateMessageRetries(msgId, retriesLeft, DateTime.Now);
		}
		/// <summary>
		/// Finds a row in the dataset with the specified identifier
		/// </summary>
		/// <param name="msgId">The identifier of the message to find</param>
		/// <returns>The row containing the message with the specified id</returns>
		protected DataRow GetMessageRow(decimal msgId)
		{
			DataRow row = null;
			string filter = string.Format("MSG_ID={0}", msgId);
			DataRow[] rows =  _msgDataSet.Tables[0].Select(filter);
			if (rows != null && rows.Length > 0)
			{
				row = rows[0];
			}
			return row;
		}
		/// <summary>
		/// Marks as failed any row in the messages dataset that has not
		/// been marked as Sent
		/// </summary>
		protected void FailPendingMessages()
		{
			int rowCount = _msgDataSet.Tables[0].Rows.Count;
			for (int i = 0; i < rowCount; i++)
			{
				MessageStatus status = 
					(MessageStatus)(int)(decimal)_msgDataSet.Tables[0].Rows[i][
						(int)MessagesObject.MsgsColumns.Status];
				if ( status == MessageStatus.Sending || status == MessageStatus.Pending) 
					_msgDataSet.Tables[0].Rows[i][(int)MessagesObject.MsgsColumns.Status] = 
						(decimal)(int) MessageStatus.Failed;
			}
		}
		/// <summary>
		/// Starts listening for a MessageProcessor events
		/// </summary>
		/// <param name="proc"></param>
		protected void RegisterProcessor(MessageProcessor proc)
		{
			proc.Done += new MessageProcessor.DoneHandler(OnRequestDone);
			proc.MessageRetried += new MessageProcessor.MessageRetriedHandler(OnMessageRetried);
			proc.MessageArrived += new MessageProcessor.MessageArrivedHandler(OnRequestResponse);
		}
		/// <summary>
		/// Stops listening for a MessageProcessor events
		/// </summary>
		/// <param name="proc"></param>
		protected void UnregisterProcessor(MessageProcessor proc)
		{
			proc.Done -= new MessageProcessor.DoneHandler(OnRequestDone);
			proc.MessageRetried -= new MessageProcessor.MessageRetriedHandler(OnMessageRetried);
			proc.MessageArrived -= new MessageProcessor.MessageArrivedHandler(OnRequestResponse);
		}
		/// <summary>
		/// Returns the message status corresponding to a request outcome
		/// </summary>
		/// <param name="outcome">The request outcome</param>
		/// <returns>The corresponding message status</returns>
		protected MessageStatus GetMessageStatus(RequestOutcome outcome)
		{
			MessageStatus status = MessageStatus.Failed;
			switch (outcome)
			{
				case RequestOutcome.SendOK:
				{
					status = MessageStatus.Sent;
					break;
				}
				case RequestOutcome.ServerNackError:
				{
					status = MessageStatus.NackServer;
					break;
				}
				case RequestOutcome.WrongMsgFormatNack:
				{
					status = MessageStatus.NackMessage;
					break;
				}
				case RequestOutcome.MessageError:
				case RequestOutcome.OperationCancelled:
				case RequestOutcome.ReceiveError:
				case RequestOutcome.SendError:
				case RequestOutcome.SendTimeout:
				{
					status = MessageStatus.Failed;
					break;
				}
			}
			return status;
		}

		#endregion // private methods

		#region Private methods
		#endregion // Private methods
		
		#region Protected data members

		/// <summary>
		/// The dataset that contains the messages to send
		/// </summary>
		protected DataSet _msgDataSet;
		/// <summary>
		/// Registers the message done handlers
		/// </summary>
		protected MessageDoneHandler _msgDoneHandler;
		/// <summary>
		/// Registers the message arrived handlers
		/// </summary>
		protected MessageArrivedHandler _msgArrivedHandler;
		/// <summary>
		/// The media context to query for media availability
		/// </summary>
		protected CommMediaContext _mediaCtx;
		/// <summary>
		/// The provider of channels
		/// </summary>
		protected ChannelManager _channelMgr;
		/// <summary>
		/// A helper object to locate messages in the dataset
		/// </summary>
		protected MessagesObject _messages;
		/// <summary>
		/// The object that manages the messages responses
		/// </summary>
		protected IMessageRouter _responseRouter;
		/// <summary>
		/// An identifier of the logical device that sends requests
		/// through this sender
		/// </summary>
		protected string _originId;
		/// <summary>
		/// An array containing the MessageProcessors that process
		/// the messages sent through this sender
		/// </summary>
		protected MessageProcessor[] _processorsInUse;
		/// <summary>
		/// The number of requests issued not finalized yet
		/// </summary>
		protected int _requestsInProcess;

		#endregion // Protected data members

		#region Private data members
		#endregion // Private data members
	}
}
