using System;
using OPS.Comm;
using OPS.Comm.Messaging;

namespace OPS.Comm.Fecs
{
	/// <summary>
	/// A channel implementation that routes messages to another IChannel 
	/// and routes responses to a queue adapter
	/// </summary>
	public class FecsChannel : IChannel
	{
		#region Public API

		/// <summary>
		/// Constructor receiving the channel to route messages to
		/// </summary>
		/// <param name="outgoingChannel">The channel to route messages to</param>
		public FecsChannel(IChannel outgoingChannel)
		{
			_outgoingChannel = outgoingChannel;
			_outgoingChannel.IncomingMessage += 
				new MessageHandler(OnOutgoingChannelIncomingMessage);
			_outgoingChannel.OutcomingMessage += 
				new MessageHandler(OnOutgoingChannelOutcomingMessage);
			_outgoingChannel.ReceiveError += 
				new ErrorHandler(OnOutgoingChannelReceiveError);
			_outgoingChannel.SendError += 
				new ErrorHandler(OnOutgoingChannelSendError);
		}

		public string Uri
		{
			get { return _uri; }
			set { _uri = value; }
		}

		public void SendMessage(OPSTelegrama msg)
		{
			_outgoingChannel.SendMessage(msg);
		}

		public void Close()
		{
			_outgoingChannel.IncomingMessage -= 
				new MessageHandler(OnOutgoingChannelIncomingMessage);
			_outgoingChannel.OutcomingMessage -= 
				new MessageHandler(OnOutgoingChannelOutcomingMessage);
			_outgoingChannel.ReceiveError -= 
				new ErrorHandler(OnOutgoingChannelReceiveError);
			_outgoingChannel.SendError -= 
				new ErrorHandler(OnOutgoingChannelSendError);
			_outgoingChannel.Close();
			_outgoingChannel = null;
		}

		public event MessageHandler OutcomingMessage;
		
		public event ErrorHandler SendError;

		public event MessageHandler IncomingMessage;

		public event ErrorHandler ReceiveError;

		#endregion // Public API
		
		#region // Private methods

		private void OnOutgoingChannelIncomingMessage(OPS.Comm.OPSTelegrama msg, IChannel channel)
		{
			if (IncomingMessage != null)
				IncomingMessage(msg, this);
		}

		private void OnOutgoingChannelOutcomingMessage(OPS.Comm.OPSTelegrama msg, IChannel channel)
		{
			if (OutcomingMessage != null)
				OutcomingMessage(msg, this);
		}

		private void OnOutgoingChannelReceiveError(int error, string message, IChannel channel)
		{
			if (ReceiveError != null)
				ReceiveError(error, message, this);
		}

		private void OnOutgoingChannelSendError(int error, string message, IChannel channel)
		{
			if (SendError != null)
				SendError(error, message, this);
		}

		#endregion // Private methods

		#region // Private data members

		/// <summary>
		/// The channel to route messages to
		/// </summary>
		private IChannel _outgoingChannel;
		/// <summary>
		/// The logical identifier for the channel
		/// </summary>
		private string _uri;

		#endregion // Private data members
	}
}
