using System;
using System.Collections;
using OPS.Comm;
using System.Threading;
using System.Diagnostics;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Adapts the sending/receiving operations of a channel to those of a request
	/// by using a timeout
	/// </summary>
	public class RequestChannelAdapter
	{
		#region Public API

		/// <summary>
		/// The signature for handlers of the MessageResponse event
		/// </summary>
		/// <param name="outcome">Indicates the success or failure of the sending opertion</param>
		/// <param name="response">If outcome indicates success, it contains the
		/// response to the message sent. If the outcome indicates failure, it is null</param>
		public delegate void ResponseHandler(
			RequestOutcome outcome, OPSTelegrama response);
		
		public delegate void RequestDoneHandler(bool timedout);

		public RequestChannelAdapter(IChannel channel)
		{
			_channel = channel;
			InitializeChannel(_channel);
		}
		/// <summary>
		/// Sends a message and waits for the currently defined timeout
		/// </summary>
		/// <param name="msgsIds">A table containing the identifiers of the
		/// messages packed in msg. Only keys matter. Keys are of type
		/// System.Decimal</param>
		/// <param name="msg">The message to be sent</param>
		public void SendMessage(Hashtable msgsIds, OPSTelegrama msg)
		{
			_messageCount = msgsIds.Count;
			_msgsIds = msgsIds;
			_waiting = true;
			try
			{
				_channel.SendMessage(msg);
			}
			catch (Exception)
			{
				DoNotify(new NotificationArgs(RequestOutcome.SendError, null));
			}
		}
		/// <summary>
		/// Releases any resources used by the adapter
		/// </summary>
		public void Close()
		{
			UninitializeChannel(_channel);
		}
		/// <summary>
		/// The event fired each time a response comes in
		/// </summary>
		public event ResponseHandler MessageResponse
		{
			add { _responseHandler += value; }
			remove { _responseHandler -= value; }
		}
		/// <summary>
		/// The event that fires when the timeout expires or all the
		/// messages have received a response
		/// </summary>
		public event RequestDoneHandler RequestDone;

		#endregion // Public API

		#region Private methods

		/// <summary>
		/// Handler for the channel IncomingMessage event
		/// </summary>
		/// <param name="msg">The received message</param>
		/// <param name="channel">The channel that received the message</param>
		private void OnChannelIncomingMessage(OPSTelegrama msg, IChannel channel)
		{
			if (_waiting)
			{
//				ThreadPool.QueueUserWorkItem(new WaitCallback(NotifyThreadProc), 
//					new NotificationArgs(RequestOutcome.SendOK, msg));
				DoNotify(new NotificationArgs(RequestOutcome.SendOK, msg));
			}
		}
		/// <summary>
		/// Handler for the channel OutcomingMessage event
		/// </summary>
		/// <param name="msg">The message sent</param>
		/// <param name="channel">The channel through wich the message was sent</param>
		private void OnChannelOutcomingMessage(OPSTelegrama msg, IChannel channel)
		{
			Debug.WriteLine("RequestAdapter - OnChannelOutcomingMessage:\n" + msg.XmlData);
		}
		/// <summary>
		/// Handler for the channel ReceiveError event
		/// </summary>
		/// <param name="error">Error code</param>
		/// <param name="message">Error description</param>
		/// <param name="channel">The channel signaling the receive error</param>
		private void OnChannelReceiveError(int error, string message, IChannel channel)
		{
			if (_waiting)
			{
//				ThreadPool.QueueUserWorkItem(new WaitCallback(NotifyThreadProc), 
//					new NotificationArgs(RequestOutcome.ReceiveError, null));
				DoNotify(new NotificationArgs(RequestOutcome.ReceiveError, null));
			}
		}
		/// <summary>
		/// Handler for the channel SendError event
		/// </summary>
		/// <param name="error">Error code</param>
		/// <param name="message">Error description</param>
		/// <param name="channel">The channel signaling the receive error</param>
		private void OnChannelSendError(int error, string message, IChannel channel)
		{
			if (_waiting)
			{
//				ThreadPool.QueueUserWorkItem(new WaitCallback(NotifyThreadProc), 
//					new NotificationArgs(RequestOutcome.SendError, null));
				DoNotify(new NotificationArgs(RequestOutcome.SendError, null));
			}
		}
		/// <summary>
		/// Handles notifications
		/// </summary>
		/// <param name="args">The notification data</param>
		private void DoNotify(NotificationArgs args)
		{
			if (args.Outcome == RequestOutcome.SendOK)
			{
				bool notify = true;
				MessageAccess msgAccess = new MessageAccess(args.Response.XmlData);
				string msgId = msgAccess.GetMessageId();
				if (msgId != null && msgId.Length > 0)
				{
					if (_msgsIds.ContainsKey(Convert.ToDecimal(msgId)))
					{
						string msgName = msgAccess.GetMessageName();
						notify = HandleAcks(msgName, ref args);
						if (notify)
						{
							string str = string.Format("RequestAdapter - About to notify response: {0}, {1}", 
								args.Outcome, args.Response.XmlData);
							Debug.WriteLine(str);
							if (_responseHandler != null)
								_responseHandler(args.Outcome, args.Response);
					
							_messageCount--;
							if (_messageCount == 0)
							{
								_waiting = false;
							}
						}
						else
						{
							string str = string.Format("RequestChannelAdapter.NotifyThreadProc - ACK not notified: {0}", 
								args.Response.XmlData);
							Debug.WriteLine(str);
						}
					}
				}
				else
				{
					string str = string.Format("RequestChannelAdapter.NotifyThreadProc - Bad message: {0}", 
						args.Response.XmlData);
					Debug.WriteLine(str);
				}
			}
			else
			{
				string str = string.Format("RequestChannelAdapter.NotifyThreadProc - Notifying error: {0}", 
					args.Outcome);
				Debug.WriteLine(str);
				if (_responseHandler != null)
					_responseHandler(args.Outcome, null);
				
				UninitializeChannel(_channel);
				string uri = _channel.Uri;
				try
				{
					str = string.Format("RequestChannelAdapter.NotifyThreadProc - Closing channel: {0}", 
						uri);
					Debug.WriteLine(str);
					Parameterization.ChannelManager.CloseChannel(uri);
/*					str = string.Format("RequestChannelAdapter.NotifyThreadProc - Opening channel: {0}", 
						uri);
					Debug.WriteLine(str);
					_channel = Parameterization.ChannelManager.OpenChannel(uri);
					InitializeChannel(_channel);
					str = string.Format("RequestChannelAdapter.NotifyThreadProc - Reattached to channel: {0}", 
						uri);
					Debug.WriteLine(str);*/
				}
				catch (Exception ex)
				{
					//str = string.Format("RequestChannelAdapter.NotifyThreadProc - Error reopening channnel: {0}", ex.Message);
					//Debug.WriteLine(str);
					CommMain.Logger.AddLog(ex);
				}
			}
		}
		/// <summary>
		/// Callback method that notifies handlers of the response event
		/// </summary>
		/// <param name="state">It is a NotificationArgs object</param>
		private void NotifyThreadProc(object state)
		{
			NotificationArgs args = (NotificationArgs) state;
			DoNotify(args);
		}
		/// <summary>
		/// Callback method that notifies handlers of the response event
		/// </summary>
		/// <param name="state">It is a boolean indicating whether the request
		/// timedd out or not</param>
		private void NotifyDoneThreadProc(object state)
		{
			if (RequestDone != null)
			{
				bool timedOut = (bool) state;
				RequestDone(timedOut);
			}
		}
		/// <summary>
		/// Checks a message name and if it's an ACK, decides the outcome
		/// for the message it refers to
		/// </summary>
		/// <param name="msgName">The name of the message</param>
		/// <param name="outcome">The outcome if the message is an ACK</param>
		/// <returns>true if the message should be notified to the sender, 
		/// false otherwise</returns>
		private bool HandleAcks(string msgName, ref NotificationArgs args)
		{
			bool notify = true;
			switch (msgName)
			{
				case Tags.AckError:
				{
					args.Outcome = RequestOutcome.MessageError;
					break;
				}
				case Tags.NackMsg:
				{
					args.Outcome = RequestOutcome.WrongMsgFormatNack;
					break;
				}
				case Tags.NackServer:
				{
					args.Outcome = RequestOutcome.ServerNackError;
					break;
				}
				case Tags.AckJammed:
				{
					args.Outcome = RequestOutcome.ServerBusy;
					break;
				}
				case Tags.AckOK:
				case Tags.AckDeferred:
				{
					notify = false;
					break;
				}
			}
			return notify;
		}
		/// <summary>
		/// Register with the channel to receive its event notifications
		/// </summary>
		/// <param name="channel">The channel to register with</param>
		private void InitializeChannel(IChannel channel)
		{
			if (channel != null)
			{
				channel.IncomingMessage += new MessageHandler(OnChannelIncomingMessage);
				channel.OutcomingMessage += new MessageHandler(OnChannelOutcomingMessage);
				channel.ReceiveError += new ErrorHandler(OnChannelReceiveError);
				channel.SendError += new ErrorHandler(OnChannelSendError);
			}
		}
		/// <summary>
		/// Removes the event notification handlers from the channel
		/// </summary>
		/// <param name="channel">The channel to unregister from</param>
		private void UninitializeChannel(IChannel channel)
		{
			if (channel != null)
			{
				channel.IncomingMessage -= new MessageHandler(OnChannelIncomingMessage);
				channel.OutcomingMessage -= new MessageHandler(OnChannelOutcomingMessage);
				channel.ReceiveError -= new ErrorHandler(OnChannelReceiveError);
				channel.SendError -= new ErrorHandler(OnChannelSendError);
			}
		}

		#endregion // Private methods

		#region Private data members

		IChannel _channel;
		ResponseHandler _responseHandler;
		int _messageCount;
		bool _waiting;
		Hashtable _msgsIds;

		#endregion // Private data members
	}

	internal class NotificationArgs
	{
		internal NotificationArgs(RequestOutcome outcome, OPSTelegrama response)
		{
			if (response != null)
				_response = OPSTelegramaFactory.CreateOPSTelegrama(response.FullData);
			_outcome = outcome;
		}
		internal RequestOutcome Outcome
		{
			get { return _outcome; }
			set { _outcome = value; }
		}
		internal OPSTelegrama Response
		{
			get { return _response; }
			set { _response = value; }
		}
		private RequestOutcome _outcome;
		private OPSTelegrama _response;
	}
}
