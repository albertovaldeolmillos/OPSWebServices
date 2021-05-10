using System;
using System.Windows.Forms;
using System.Text;
using System.Collections;
using System.Threading;
using System.Diagnostics;
using OPS.Comm;
using System.Configuration;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// The possible outcomes of a request
	/// </summary>
	public enum RequestOutcome 
	{
		SendOK, SendTimeout, SendError, ReceiveError, OperationCancelled,
		ServerNackError, WrongMsgFormatNack, MessageError, ServerBusy 
	}
	/// <summary>
	/// The possible status a message can be in
	/// </summary>
	public enum MessageStatus
	{
		Pending, Sending, Sent, Failed, Jammed, Error, NackMessage, NackServer,
		TimedOut, SendFailed, NoMedia
	}

	/// <summary>
	/// Sends messages that need a response in a reliable manner. 
	/// Sends a packet containing a group of messages following a common
	/// retry policy until they are successfuly sent, fail or timeout
	/// </summary>
	public class Request
	{
		#region Public API
		
		/// <summary>
		/// The signature for handlers of the MessageResponse event
		/// </summary>
		public delegate void ResponseHandler(
			decimal msgId, RequestOutcome outcome, OPSTelegrama response);
		/// <summary>
		/// The signature for handlers of the MessageRetry event
		/// </summary>
		public delegate void RetryHandler(
			decimal msgId, decimal retriesLeft);
		/// <summary>
		/// The signature for handlers of the RequestDone event
		/// </summary>
		public delegate void RequestDoneHandler(Request sender);

		public Request(IChannel channel)
		{
			_maxPacketSize = 4096; // TODO: Provide better estimation and pick it up from a parameter
			_channelAdapter = new RequestChannelAdapter(channel);
			_channelAdapter.MessageResponse += 
				new RequestChannelAdapter.ResponseHandler(
					OnChannelAdapterMessageResponse);
			_channelAdapter.RequestDone += 
				new RequestChannelAdapter.RequestDoneHandler(OnChannelAdapterRequestDone);
			_evtProceed = new ManualResetEvent(false);
			_timerTimeouts = new System.Threading.Timer(new TimerCallback(OnTimeout), this,
				Timeout.Infinite, Timeout.Infinite);
			_sending = false;
			_cancelling = false;
			_originId = "???";
		}
		/// <summary>
		/// Sends a group of messages in a reliable manner
		/// </summary>
		/// <param name="msgStatus">A table that contains the data for each
		/// message to be sent in this request</param>
		/// <returns>true if the sending process is initiated, 
		/// false otherwise</returns>
		public bool Send(MessageDataTable msgStatus)
		{
			bool retVal = false;
			if (!_sending && msgStatus != null)
			{
				_msgStatus = msgStatus;
				_messagesLeft = msgStatus.Count;
				if (_messagesLeft > 0)
				{
					IEnumerator en = _msgStatus.Values.GetEnumerator();
					en.MoveNext();
					MessageData msg = (MessageData) en.Current;
					_totalTimeout = Convert.ToInt32(msg.TotalTimeout);
					_totalRetries = msg.TotalRetries;
					_totalRetriesPending = _totalRetries;
					_partialRetries = msg.PartialRetries;
					UpdateWithMediaAvailability();
					ThreadPool.QueueUserWorkItem(
						new WaitCallback(MainSenderThreadProc));
					retVal = true;
				}
			}
			return retVal;
		}
		/// <summary>
		/// Cancels the current request
		/// </summary>
		/// <remarks>The cancelation delay could be determined by the 
		/// retry timeouts in use</remarks>
		public void Cancel()
		{
			_sending = false;
			_cancelling = true;
		}
		/// <summary>
		/// An identifier of the logical device that sends this
		/// request
		/// </summary>
		public string OriginId
		{
			get { return _originId; }
			set { _originId = value; }
		}
		/// <summary>
		/// The event raised when a response is available for a message or an
		/// error has ocurred sending the message
		/// </summary>
		public event ResponseHandler MessageResponse
		{
			add { _responseHandler += value; }
			remove { _responseHandler -= value; }
		}
		/// <summary>
		/// The event fired when a retry of a message has been issued
		/// </summary>
		public event RetryHandler MessageRetry
		{
			add { _retryHandler += value; }
			remove { _retryHandler -= value; }
		}
		public event RequestDoneHandler RequestDone
		{
			add { _requestDoneHandler += value; }
			remove { _requestDoneHandler -= value; }
		}
		/// <summary>
		/// To call when the request is not longer needed
		/// </summary>
		public void Close()
		{
			if (_channelAdapter != null)
			{
				_channelAdapter.MessageResponse -= 
					new RequestChannelAdapter.ResponseHandler(
					OnChannelAdapterMessageResponse);
				_channelAdapter.RequestDone -= 
					new RequestChannelAdapter.RequestDoneHandler(OnChannelAdapterRequestDone);

				_channelAdapter.Close();
				_channelAdapter = null;
			}
			/// Fail-safe closing
			_retryHandler = null;
			_responseHandler = null;
			_requestDoneHandler = null;
			if (_evtProceed != null)
			{
				_endWaitAfterClosing = true;
				_evtProceed.Set();
			}
			//>> LDR 2004.05.07
			_timerTimeouts = null;
			//<< LDR 2004.05.07
		}
		/// <summary>
		/// The maximum allowed size for a packet
		/// </summary>
		public int MaxPacketSize
		{
			get { return _maxPacketSize; }
			set { _maxPacketSize = value; }
		}

		#endregion // Public API

		#region Private methods

		/// <summary>
		/// Builds a packet containing only the messages that need to be
		/// retried. The status of the messages is set to Sending
		/// </summary>
		/// <param name="msg">The object that receives the packet</param>
		/// <param name="msgIds">The identifiers of the messages in the packet. Only keys
		/// are important</param>
		/// <returns>The number of messages packed</returns>
		private int PrepareMessagesToSend(out OPSTelegrama msg, out Hashtable msgIds)
		{
			msg = null;
			StringBuilder packet = null;
			int msgCount = GetMessagesPacket(out packet, out msgIds);
			Debug.WriteLine("Request - Messages in packet: " + msgCount);
			if (msgCount > 0 && packet != null)
			{
				msg = OPSTelegramaFactory.CreateOPSTelegrama(Correlation.NextId, packet.ToString());
			}
			return msgCount;
		}
		/// <summary>
		/// Groups the messages that need to be sent. Each message added to
		/// the packet is marked as "Sending"
		/// </summary>
		/// <param name="packet">The resulting packet</param>
		/// <param name="msgIds">The identifiers of the messages in the packet. Only keys
		/// are important</param>
		/// <returns>The number of messages in the packet</returns>
		private int GetMessagesPacket(out StringBuilder packet, out Hashtable msgIds)
		{
			packet = null;
			msgIds = null;
			int msgCount = 0;
			int currentSize = _packetOverhead;
			lock (_msgStatus)
			{

				AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				double nDifHour=0;
				try
				{
					nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
				}
				catch
				{
					nDifHour=0;
				}		
				
				bool enoughData = false;
				IEnumerator en = _msgStatus.Values.GetEnumerator();
				while (en.MoveNext() && !enoughData)
				{
					MessageData msg = (MessageData) en.Current;
					// Before atempting to include the message, see if it has
					// already expired
					FailMessageIfExpired(msg, DateTime.Now.AddHours(nDifHour));
					// Send only messages not successfuly sent nor failed
					// and still having pending retries
					if (msg.Status != MessageStatus.Sent &&
						msg.Status != MessageStatus.Failed &&
						msg.Status != MessageStatus.SendFailed &&
						msg.Status != MessageStatus.NoMedia &&
						msg.PendingRetries > 0)
					{
						if (packet == null)
						{
							packet = new StringBuilder(256);
							packet.AppendFormat(null, "<{0} {1}=\"{2}\" {3}=\"{4}\">",
								Tags.Packet, Tags.PacketSrcAttr, _originId,
								Tags.PacketDateAttr, Dtx.DtxToString(DateTime.Now.AddHours(nDifHour)));
							msgIds = new Hashtable(_msgStatus.Count);
						}
						// Add the message only if it doesn't make the packet
						// exceed the maximum size
						if (currentSize + msg.XmlData.Length < _maxPacketSize)
						{
							packet.Append(msg.XmlData);
							msgIds.Add(msg.MessageId, msg.MessageId);
							msg.Status = MessageStatus.Sending;
							msgCount++;
							currentSize += msg.XmlData.Length;
						}
						else
						{
							enoughData = true;
						}
					}
				}
				if (packet != null)
				{
					packet.AppendFormat(null, "</{0}>", Tags.Packet);
				}
			}
			return msgCount;
		}
		
		/// <summary>
		/// Handler for the ChannelAdapter MessageResponse event
		/// </summary>
		/// <param name="outcome">The outcome of the message</param>
		/// <param name="response">The response received when the
		/// outcome indicates success, null otherwise</param>
		private void OnChannelAdapterMessageResponse(RequestOutcome outcome, 
			OPSTelegrama response)
		{
			if (_sending && response != null && _responseHandler != null)
			{
				ThreadPool.QueueUserWorkItem(
					new WaitCallback(HandleResponseThreadProc), 
					new RequestResponse(outcome, response));
			}
			else
			{
				if (outcome == RequestOutcome.ReceiveError || 
					outcome == RequestOutcome.SendError)
				{
					Debug.WriteLine("Request - Finalizing request due to receive or send error");
					FinalizeRequest();
				}
			}
		}
		/// <summary>
		/// Handler for the ChannelAdapter RequestDone event
		/// </summary>
		/// <param name="timedout">Indicates whether the request
		/// finished because of a timeout or because all the messages
		/// received a response</param>
		private void OnChannelAdapterRequestDone(bool timedout)
		{
			Debug.WriteLine("Request - ChannelAdapter timedout: " + timedout.ToString());
			_timedOut = timedout;
			_evtProceed.Set();
		}
		/// <summary>
		/// Changes the status of a message in a thread-safe way
		/// </summary>
		/// <param name="msgId">The identifier of the message to update</param>
		/// <param name="status">The new status of a message</param>
		private void UpdateMessageStatus(decimal msgId, MessageStatus status)
		{
			lock (_msgStatus)
			{
				MessageData msg = _msgStatus[msgId];
				if (msg != null)
				{
					msg.Status = status;
				}
			}
		}
		/// <summary>
		/// Annotates a new retry to all pending messages
		/// </summary>
		private void UpdateMessageRetries()
		{
			lock (_msgStatus)
			{
				AppSettingsReader appSettings = new System.Configuration.AppSettingsReader();
				double nDifHour=0;
				try
				{
					nDifHour= (double) appSettings.GetValue   ("HOUR_DIFFERENCE",typeof(double));
				}
				catch
				{
					nDifHour=0;
				}
			
				DateTime now = DateTime.Now.AddHours(nDifHour);
				IEnumerator en = _msgStatus.Values.GetEnumerator();
				while (en.MoveNext())
				{
					MessageData msg = (MessageData) en.Current;
					if (msg.Status == MessageStatus.Sending)
					{
						msg.PendingRetries--;
						FireRetryHandlers(msg);
						//FailMessageIfExpired(msg, now);
					}
				}
			}
		}
		/// <summary>
		/// Marks a message as Failed if its expiration time is before
		/// the supplied time
		/// </summary>
		/// <param name="msg">The message which expiration time is checked</param>
		/// <param name="time">The time to check against the message
		/// expiration time</param>
		private void FailMessageIfExpired(MessageData msg, DateTime time)
		{
			TimeSpan elapsedSpan = time.Subtract(msg.SentTime);
			TimeSpan allowedSpan = TimeSpan.FromSeconds(
				Convert.ToDouble(msg.ExpirationTime));

			if (elapsedSpan >= allowedSpan)
				msg.Status = MessageStatus.Failed;
		}
		/// <summary>
		/// The thread procedure in charge of sending messages following the
		/// retry policy
		/// </summary>
		/// <param name="state">Unused</param>
		private void MainSenderThreadProc(object state)
		{
			_cancelling = false;
			bool requestDone = false;
			// Total retries loop
			while (_messagesLeft > 0 && _totalRetriesPending > 0 &&
				!_cancelling && !requestDone)
			{
				_timedOut = false;
				_partialRetriesPending = _partialRetries;
				// Partial retries loop
				while (_messagesLeft > 0 && _partialRetriesPending > 0 &&
					!_cancelling && !requestDone)
				{
					OPSTelegrama packet = null;
					Hashtable msgIds = null;
					_messagesLeft = PrepareMessagesToSend(out packet, out msgIds);
					if (_messagesLeft > 0)
					{
						_sending = true;
						Debug.WriteLine("Request - About to retry");
						UpdateMessageRetries();
						try
						{
							_channelAdapter.SendMessage(msgIds, packet);
							
							WaitForCompletion();
							if (_timedOut)
							{
								Debug.WriteLine("Request - Partial timeout");
								_sending = false;
								_timedOut = false;
							}
							else
							{
								requestDone = true;
							}
						}
						catch (Exception ex)
						{
							//Debug.WriteLine("Request.MainSenderThreadProc - SendMessage failed: " + ex.Message);
							CommMain.Logger.AddLog(ex);
							UpdateMessageStatus(msgIds, MessageStatus.SendFailed);
						}
					}
					_partialRetriesPending--;
				}
				if (_messagesLeft > 0)
				{
					WaitForCompletion(_totalTimeout);
					if (_timedOut)
					{
						Debug.WriteLine("Request - Total timeout");
						_timedOut = false;
					}
				}
				_totalRetriesPending--;
			}
			FinalizeRequest();
		}
		/// <summary>
		/// Callback to call when a timeout occurs
		/// </summary>
		/// <param name="state"></param>
		private void OnTimeout(object state)
		{
			_timedOut = true;
			_evtProceed.Set();
		}
		/// <summary>
		/// Waits the specified number of milliseconds for the request
		/// to complete or timeout
		/// </summary>
		/// <param name="timeout">Maximum time allowed for completion</param>
		private void WaitForCompletion(int timeout)
		{
			if (_timerTimeouts != null) // It's null after a close
				_timerTimeouts.Change(timeout, Timeout.Infinite);
			_evtProceed.Reset();
			_evtProceed.WaitOne();
		}
		/// <summary>
		/// Waits for the request to complete or timeout
		/// </summary>
		private void WaitForCompletion()
		{
			_evtProceed.Reset();
			_evtProceed.WaitOne();
			if (_endWaitAfterClosing)
				Debug.WriteLine("Request.WaitForCompletion ending after Close");
		}
		/// <summary>
		/// Handles the arrival of a response to a message
		/// </summary>
		/// <param name="state">Is a RequestResponse object</param>
		private void HandleResponseThreadProc(object state)
		{
			RequestResponse response = (RequestResponse) state;
			if (response.Message != null)
			{
				Debug.WriteLine("Request - Handling response");
				MessageAccess msg = new MessageAccess(response.Message.XmlData);
				string msgId = msg.GetMessageId();
				if (msgId != null)
				{
					_messagesLeft--;
					string str = string.Format("Request - Response to {0}, {1}",
						msgId, response.Outcome);
					Debug.WriteLine(str);
					decimal id = decimal.Parse(msgId);
					UpdateMessageStatus(id, 
						(response.Outcome == RequestOutcome.SendOK) ? 
						MessageStatus.Sent : MessageStatus.Failed);
					if (_responseHandler != null)
						_responseHandler(id, response.Outcome, response.Message);
					if (_messagesLeft == 0)
						FinalizeRequest();
				}
			}
		}
		/// <summary>
		/// Starts a thread to callback the retry messagae event handlers
		/// </summary>
		/// <param name="msg">The message that will be retried</param>
		private void FireRetryHandlers(MessageData msg)
		{
			ThreadPool.QueueUserWorkItem(new WaitCallback(
				FireRetryHandlersThreadProc), msg);
		}
		/// <summary>
		/// Fires the MessageRetry event handlers
		/// </summary>
		/// <param name="state">Is a MessageData object</param>
		private void FireRetryHandlersThreadProc(object state)
		{
			if (_retryHandler != null)
			{
				MessageData msg = (MessageData) state;
				_retryHandler(msg.MessageId, msg.PendingRetries);
			}
		}
		/// <summary>
		/// Fires the RequestDone event handlers
		/// </summary>
		private void FireRequestDoneHandlers()
		{
			_sending = false;
			if (_requestDoneHandler != null)
			{
				_requestDoneHandler(this);
			}
		}
		/// <summary>
		/// Marks as timed out any message that has not been answered yet
		/// </summary>
		private void TimeOutPendingMessages()
		{
			lock (_msgStatus)
			{
				IEnumerator en = _msgStatus.Values.GetEnumerator();
				while (en.MoveNext())
				{
					MessageData msg = (MessageData) en.Current;
					if (msg.Status == MessageStatus.Sending)
						msg.Status = MessageStatus.TimedOut;
				}
			}
		}
		/// <summary>
		/// Changes the status of a series of messages being sent
		/// </summary>
		/// <param name="msgsIds">A hashtable containing the identifiers of
		/// the messages to update</param>
		/// <param name="status">The new status for the messages</param>
		private void UpdateMessageStatus(Hashtable msgsIds, MessageStatus status)
		{
			lock (_msgStatus)
			{
				IEnumerator en = msgsIds.Values.GetEnumerator();
				while (en.MoveNext())
				{
					decimal id = (decimal) en.Current;
					MessageData msg = _msgStatus[id];
					if (msg != null &&
						(msg.Status == MessageStatus.Sending || 
						 msg.Status == MessageStatus.Pending))
						msg.Status = status;
				}
			}
		}
		/// <summary>
		/// Marks the pending messages has timed out and fires the request
		/// done handlers
		/// </summary>
		private void FinalizeRequest()
		{
			lock (this)
			{
				if (!_requestFinalized)
				{
					_requestFinalized = true;
					if (_messagesLeft > 0)
					{
						TimeOutPendingMessages();
					}
					FireRequestDoneHandlers();
				}
			}
			_evtProceed.Set();
		}
		/// <summary>
		/// Marks with the media unavailable status the messages for which no
		/// media is available
		/// </summary>
		private void UpdateWithMediaAvailability()
		{
			lock (_msgStatus)
			{
				IEnumerator en = _msgStatus.Values.GetEnumerator();
				while (en.MoveNext())
				{
					MessageData msg = (MessageData) en.Current;
					int media = (int) msg.Media;
					try
					{
						Media.MediaType mediaType = (Media.MediaType) media;
						if (!Parameterization.MediaContext.IsMediaAvailable(mediaType))
						{
							msg.Status = MessageStatus.NoMedia;
						}
					}
					catch (Exception ex)
					{
						CommMain.Logger.AddLog(ex);
					}
				}
			}
		}

		#endregion // Private methods

		#region Private data members
		
		/// <summary>
		/// The handlers to call back when a response to a message arrives
		/// </summary>
		private ResponseHandler _responseHandler;
		/// <summary>
		/// The handlers to call back when a message is retried
		/// </summary>
		private RetryHandler _retryHandler;
		/// <summary>
		/// The handlers to call back when all messages have been handled
		/// </summary>
		private RequestDoneHandler _requestDoneHandler;
		/// <summary>
		/// Maps the identifiers of the messages this request is responsible for
		/// to their status
		/// </summary>
		private MessageDataTable _msgStatus;
		/// <summary>
		/// The adapter through which the messages are sent
		/// </summary>
		private RequestChannelAdapter _channelAdapter;
		/// <summary>
		/// The number of messages not successfuly sent yet
		/// </summary>
		private int _messagesLeft;
		/// <summary>
		/// The time interval between total retries. In milliseconds
		/// </summary>
		private int _totalTimeout;
		/// <summary>
		/// The number of total retries pending
		/// </summary>
		private decimal _totalRetries;
		/// <summary>
		/// The number of partial retries pending
		/// </summary>
		private decimal _partialRetries;
		/// <summary>
		/// The maximum packet size
		/// </summary>
		private int _maxPacketSize;
		/// <summary>
		/// The current number of total retries pending
		/// </summary>
		private decimal _totalRetriesPending;
		/// <summary>
		/// The current number of partial retries pending
		/// </summary>
		private decimal _partialRetriesPending;
		/// <summary>
		/// Indicates if a sending operation has already been initiated
		/// </summary>
		private bool _sending;
		/// <summary>
		/// Indicates that the request must be cancelled as soon as possible
		/// </summary>
		private bool _cancelling;
		/// <summary>
		/// Timer upon which the request waits for completion or timeouts
		/// </summary>
		private ManualResetEvent _evtProceed;
		/// <summary>
		/// The timer to control partial and total timeouts
		/// </summary>
		private System.Threading.Timer _timerTimeouts;
		/// <summary>
		/// Indicates whether a timeout has occurred or not
		/// </summary>
		private bool _timedOut;
		/// <summary>
		/// An identifier of the logical device that sends this
		/// request
		/// </summary>
		protected string _originId;
		/// <summary>
		/// The overhead of additional packet data (data not belonging
		/// to messsages)
		/// </summary>
		private const int _packetOverhead = 50; // TODO: Provide a better estimation and pick it up from a parameter
		/// <summary>
		/// Indicates whether the request done handlers have been called or not
		/// </summary>
		private bool _requestFinalized = false;
		/// <summary>
		/// For logging purposes only. If true, something went wrong because
		/// the wait for responses didn't stop until Close was called
		/// </summary>
		private bool _endWaitAfterClosing = false;

		#endregion // Private data members
	}

	internal class RequestResponse
	{
		internal RequestResponse(RequestOutcome outcome, OPSTelegrama msg)
		{
			_outcome = outcome;
			_msg = msg;
		}
		internal RequestOutcome Outcome
		{
			get { return _outcome; }
		}
		internal OPSTelegrama Message
		{
			get { return _msg; }
		}
		private RequestOutcome _outcome;
		private OPSTelegrama _msg;
	}
}
