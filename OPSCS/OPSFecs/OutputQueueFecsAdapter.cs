using System;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// An interface adaptation between MSMQ queue output wrappers and
	/// communication channels
	/// </summary>
	public class OutputQueueFecsAdapter
	{
		#region Public API
		/// <summary>
		/// Constructor taking the object to use as interface to an outbound
		/// message queue
		/// </summary>
		/// <param name="queue">The outbound message queue</param>
		/// <param name="validator">The object that knows how to validate 
		/// incoming messages</param>
		public OutputQueueFecsAdapter(FecsBecsOutputWrapper queue, IMessageValidator validator)
		{
			_outQueue = queue;
			_validator = validator;
		}

		/// <summary>
		/// Handler for the IChannel.IncomingMessage event
		/// </summary>
		/// <param name="msg">The message received</param>
		/// <param name="channel">The channel that received the message</param>
		public void OnChannelIncomingMessage(OPSTelegrama msg, IChannel channel)
		{
			if (ValidateMessage(msg))
			{
				string ip;
				int port;
				TcpIpUri.UriAsAddressPort(channel.Uri, out ip, out port);
				IPacketizer packetizer = msg.Packetizer;
				FecsBecsHeader hd = new FecsBecsHeader(channel.Uri, ip, packetizer.PacketInfo);
				for (int i = 0; i < packetizer.PacketsCount; i++)
				{
					_outQueue.Send(hd, packetizer[i]);
					FecsMain.Logger.AddLog(
						string.Format("[OutputQueueFecsAdapter]:Message received from {0} [{1}]", channel.Uri, msg.XmlData), 
						LoggerSeverities.Info);
				}
			}
			else
			{
				FecsMain.Logger.AddLog(
					string.Format("[OutputQueueFecsAdapter]:Message not validated from {0} [{1}]", channel.Uri, msg.XmlData), 
					LoggerSeverities.Error);
				NackMessage(msg, channel);
			}
		}
		/// <summary>
		/// Handler for the IChannel.ReceiveError event
		/// </summary>
		/// <param name="error">The error code</param>
		/// <param name="message">The error message</param>
		/// <param name="channel">The channel signaling the error</param>
		public void OnChannelReceiveError(int error, string message, IChannel channel)
		{
			string uri = channel.Uri;
			if (error == (int) SocketChannel.SocketErrors.RemoteClosed)
			{
				CommObjects.ChannelManager.CloseChannel(uri);
				FecsMain.Logger.AddLog(string.Format("Client {0} disconnected (outqueue)", uri), 
					LoggerSeverities.Info);
				//>> LDR 2004.05.05
				string ip;
				int port;
				TcpIpUri.UriAsAddressPort(channel.Uri, out ip, out port);
				FecsBecsHeader hd = new FecsBecsHeader(channel.Uri, ip, new PacketData(System.DateTime.Now, "-1"));
				_outQueue.Send(hd, "<s1 id=\"0\"><uri>" + channel.Uri + "</uri></s1>");
				//<< LDR 2004.05.05

				//>> LDR 2004.07.16
				//				channel.SendError -= new ErrorHandler(OnChannelSendError);
				//				channel.IncomingMessage -= 
				//					new MessageHandler(OnChannelIncomingMessage);
				//				channel.ReceiveError -= 
				//					new ErrorHandler(OnChannelReceiveError);
				//<< LDR 2004.07.16
			}
			else if (CommObjects.ChannelManager.GetChannel(uri) != null)
			{
				FecsMain.Logger.AddLog(string.Format("[{2}]Error receiving from {0}: {1} ({2})",
					channel.Uri, error, message), LoggerSeverities.Error);
			}
		}

		//>> LDR 2004.07.16
		/// <summary>
		/// Handler for the IChannel.SendError event
		/// </summary>
		/// <param name="error">The error code</param>
		/// <param name="message">The error message</param>
		/// <param name="channel">The channel indicating the error</param>
		public void OnChannelSendError(int error, string message, IChannel channel)
		{
			string uri = channel.Uri;
			if (error == (int) SocketChannel.SocketErrors.RemoteClosed)
			{
				CommObjects.ChannelManager.CloseChannel(uri);
				FecsMain.Logger.AddLog(string.Format("Client {0} disconnected (inqueue)", uri), 
					LoggerSeverities.Debug);
			}
			else if (CommObjects.ChannelManager.GetChannel(uri) != null)
			{
				FecsMain.Logger.AddLog(string.Format("Error sending to {0}: {1} ({2})",
					channel.Uri, error, message), LoggerSeverities.Error);
			}
		}
		//<< LDR 2004.07.16
		#endregion // Public API
	
		#region Private methods
		
		/// <summary>
		/// Checks the correctness of an incoming message
		/// </summary>
		/// <param name="msg">The message to validate</param>
		/// <returns>true if the message is correct, false otherwise</returns>
		private bool ValidateMessage(OPSTelegrama msg)
		{
			bool retVal = msg.Correct;
			if (retVal)
			{
				if (_validator != null)
					retVal = _validator.Validate(msg);
			}
			return retVal;
		}

		/// <summary>
		/// Sends a NACK for the received message
		/// </summary>
		/// <param name="msg">The message to not acknowledge</param>
		/// <param name="channel">The channel that received the message</param>
		private void NackMessage(OPSTelegrama msg, IChannel channel)
		{
			string[] msgsId = null;
			if (msg != null)
				msgsId = msg.GetMessagesId();
			if (msgsId != null)
			{
				for (int i = 0; i < msgsId.Length; i++)
				{
					string nackBody = string.Format("<ne id=\"{0}\"/>", msgsId[i]);
					OPSTelegrama t = OPSTelegramaFactory.CreateOPSTelegrama(Correlation.NextId, nackBody);
					channel.SendMessage(t);
					FecsMain.Logger.AddLog(
						string.Format("Message sent to {0} [{1}]", channel.Uri, msg.XmlData), 
						LoggerSeverities.Debug);
				}
			}
		}

		/// <summary>
		/// Sends a NACK for the received message
		/// </summary>
		/// <param name="channel">The channel that received the message</param>
		private void NackMessage(IChannel channel)
		{
			string nackBody = "<ne id=\"0\"/>";
			OPSTelegrama t = OPSTelegramaFactory.CreateOPSTelegrama(Correlation.NextId, nackBody);
			channel.SendMessage(t);
			FecsMain.Logger.AddLog(
				string.Format("Message sent to {0} [{1}]", channel.Uri, nackBody), 
				LoggerSeverities.Debug);
		}
		#endregion // Private methods

		#region Private data members
		/// <summary>
		/// The outbound message queue
		/// </summary>
		private FecsBecsOutputWrapper _outQueue;
		/// <summary>
		/// The object that knows how to validate incoming messages
		/// </summary>
		private IMessageValidator _validator;
		#endregion // Private data members
	}
}
