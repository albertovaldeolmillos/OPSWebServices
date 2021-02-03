using System;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Xml;
using OPS.Comm;

namespace OPS.Comm.Messaging
{
	/// <summary>
	/// Handles the sending of a request and the receiving of the responses
	/// </summary>
	public class MessageProcessor
	{
		#region Public API

		/// <summary>
		/// The signature for handlers of MessageArrived event
		/// </summary>
		public delegate void MessageArrivedHandler(decimal msgId,
			RequestOutcome outcome, string msg, 
			MessageProcessor sender, string srcId);
		/// <summary>
		/// The signature for handlers of the Done event
		/// </summary>
		public delegate void DoneHandler(MessageProcessor sender);
		/// <summary>
		/// The signature for handlers of the MessageRetried event
		/// </summary>
		public delegate void MessageRetriedHandler(decimal msgId, 
			decimal retriesLeft, MessageProcessor sender);

		/// <summary>
		/// A constructor that takes the channel through which
		/// send and receive messages
		/// </summary>
		/// <param name="ch">The channel through which send and 
		/// receive messages</param>
		public MessageProcessor(IChannel ch)
		{
			_channel = ch;
			//>> LDR 2004.05.05
			//_id = string.Format("{0}-{1}", _channel.Uri, DateTime.Now.Ticks);
			_id = string.Format("{0}", _channel.Uri);
			//<< LDR 2004.05.05
			_originId = Parameterization.SourceId;
			RegisterForReceiving();
		}
		/// <summary>
		/// Sends a group of messages
		/// </summary>
		/// <param name="msgTable">The group of messages to send</param>
		public void Send(MessageDataTable msgTable)
		{
			UnregisterForReceiving();
			_msgTable = msgTable;
			_request = new Request(_channel);
			_request.OriginId = _originId;
			_request.MessageResponse += 
				new Request.ResponseHandler(OnRequestMessageResponse);
			_request.MessageRetry += new Request.RetryHandler(OnRequestMessageRetry);
			_request.RequestDone += new Request.RequestDoneHandler(OnRequestRequestDone);
			_request.Send(_msgTable);
		}
		/// <summary>
		/// Sends a message immediately and without expecting a response
		/// </summary>
		/// <param name="msg">The message to send</param>
		public void OneWaySend(string msg)
		{
			OPSTelegrama packet = OPSTelegramaFactory.CreateOPSTelegrama(Correlation.NextId, msg);
			try
			{
				_channel.SendMessage(packet);
			}
			catch (Exception ex)
			{
				//Debug.WriteLine("MsgProc OneWaySend error: " + ex.Message);
				CommMain.Logger.AddLog(ex);
			}
		}
		/// <summary>
		/// Releases any resource held by the object
		/// </summary>
		public void Close()
		{
			//>> LDR 2004.05.05
			UnregisterForReceiving();
			//<< LDR 2004.05.05
			if (_request != null)
			{
				_request.MessageResponse -= 
					new Request.ResponseHandler(OnRequestMessageResponse);
				_request.MessageRetry -= new Request.RetryHandler(OnRequestMessageRetry);
				_request.RequestDone -= new Request.RequestDoneHandler(OnRequestRequestDone);
				_request.Close();
				_request = null;
			}
			_msgTable = null;
		}
		/// <summary>
		/// Read-only property that returns the last table of messages
		/// sent
		/// </summary>
		public MessageDataTable MessageTable
		{
			get { return _msgTable; }
		}
		/// <summary>
		/// Read-only property that returns the URI of the channel through
		/// which messages are sent and received
		/// </summary>
		public string ChannelURI
		{
			get { return _channel.Uri; }
		}
		/// <summary>
		/// Read-only property that returns the unique identifier of
		/// the message processor
		/// </summary>
		public string Id
		{
			get { return _id; }
		}
		/// <summary>
		/// The retry policy for the message processor. May be null
		/// </summary>
		public RetryPolicy Policy
		{
			get { return _policy; }
			set { _policy = value; }
		}
		/// <summary>
		/// An identifier of the logical device that sends requests
		/// through this processor
		/// </summary>
		public string OriginId
		{
			get { return _originId; }
			set { _originId = value; }
		}
		/// <summary>
		/// The event that fires when a message arrives
		/// </summary>
		public event MessageArrivedHandler MessageArrived;
		/// <summary>
		/// The event that fires when all the messages have been sent or
		/// the timeout expired
		/// </summary>
		public event DoneHandler Done;
		/// <summary>
		/// The event that fires when a message is retried
		/// </summary>
		public event MessageRetriedHandler MessageRetried;

		#endregion // Public API
		
		#region Private methods
		
		private void OnRequestMessageResponse(decimal msgId, 
			RequestOutcome outcome, OPSTelegrama response)
		{
			if (MessageArrived != null)
				MessageArrived(msgId, outcome, response.XmlData, this, response.Packetizer.PacketInfo.SourceId);
			else
				CommMain.Logger.AddLog("WARNING: MessageProcessor.OnRequestMessageResponse: MessageArrived is null.", LoggerSeverities.Debug);
		}
		private void OnRequestMessageRetry(decimal msgId, 
			decimal retriesLeft)
		{
			if (MessageRetried != null)
				MessageRetried(msgId, retriesLeft, this);
			else
				CommMain.Logger.AddLog("WARNING: MessageProcessor.OnRequestMessageRetry: MessageRetried is null.", LoggerSeverities.Debug);
        }
		private void OnRequestRequestDone(Request sender)
		{
			if (Done != null)
				Done(this);
			else
				CommMain.Logger.AddLog("WARNING: MessageProcessor.OnRequestRequestDone: Done is null.", LoggerSeverities.Debug);
            //>> LDR 2004.05.07
            if (_request != null)
			{
				_request.MessageResponse -= 
					new Request.ResponseHandler(OnRequestMessageResponse);
				_request.MessageRetry -= new Request.RetryHandler(OnRequestMessageRetry);
				_request.RequestDone -= new Request.RequestDoneHandler(OnRequestRequestDone);
				_request.Close();
				_request = null;
			}
			//<< LDR 2004.05.07
			sender.Close();
		}
		private void RegisterForReceiving()
		{
			_channel.IncomingMessage += 
				new MessageHandler(OnChannelIncomingMessage);
		}
		private void UnregisterForReceiving()
		{
			_channel.IncomingMessage -= 
				new MessageHandler(OnChannelIncomingMessage);
		}
		private void OnChannelIncomingMessage(OPSTelegrama packet, IChannel channel)
		{
			if (MessageArrived != null)
			{
				if (packet != null)
				{
					XmlDocument doc = new XmlDocument();
					doc.LoadXml(packet.XmlData);
					XmlNode root = doc;
					if (root.ChildNodes.Count > 0)
					{
						/// Is a packet containing messages
						UpdateAddressCache(packet, channel.Uri);
						IPacketizer pt = packet.Packetizer;
						for (int i = 0; i < pt.PacketsCount; i++)
						{
							string msg = pt[i];
							Debug.WriteLine("MsgProc - Arrived: " + msg);
							MessageAccess acc = new MessageAccess(msg);
							string msgId = acc.GetMessageId();
							MessageArrived(Convert.ToDecimal(msgId), RequestOutcome.SendOK, 
								msg, this, packet.Packetizer.PacketInfo.SourceId);
						}
					}
					else
					{
						/// Is an ACK eventually containing response data
						MessageAccess acc = new MessageAccess(packet.XmlData);
						string msgId = acc.GetMessageId();
						MessageArrived(Convert.ToDecimal(msgId), RequestOutcome.SendOK, 
							packet.XmlData, this, packet.Packetizer.PacketInfo.SourceId);
					}
				}
			}
			else
				CommMain.Logger.AddLog("WARNING: MessageProcessor.OnChannelIncomingMessage: MessageArrived is null.", LoggerSeverities.Debug);
		}
		/// <summary>
		/// Updates the address cache with the unit of a packet source  
		/// </summary>
		/// <param name="packet">A received packet</param>
		/// <param name="uri">The uri of the channel through which the packet came</param>
		private void UpdateAddressCache(OPSTelegrama packet, string uri)
		{
			string ip;
			int port;
			TcpIpUri.UriAsAddressPort(uri, out ip, out port);
			OPSPacketizer packetizer = new OPSPacketizer(packet.XmlData);
			PacketData packData = packetizer.PacketInfo;
			if (packData != null)
			{
				string src = packData.SourceId;
				if ((src != null) && (src != ""))
				{
					Configuration.AddressCache.GetAddressCache().CacheUnitAddress(
						decimal.Parse(src), ip);
				}
			}
		}

		#endregion // Private methods

		#region Private data members

		/// <summary>
		/// The request used to sending messages
		/// </summary>
		private Request _request;
		/// <summary>
		/// The group of messages to send
		/// </summary>
		private MessageDataTable _msgTable;
		/// <summary>
		/// The channel through which messages are sent and received
		/// </summary>
		private IChannel _channel;
		/// <summary>
		/// The unique identifier of the message processor
		/// </summary>
		private string _id;
		/// <summary>
		/// The retry policy for the message processor. May be undefined
		/// </summary>
		private RetryPolicy _policy;
		/// <summary>
		/// An identifier of the logical device that sends requests
		/// through this processor
		/// </summary>
		private string _originId;

		#endregion // Private data members
	}
}
